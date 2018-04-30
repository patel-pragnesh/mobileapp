using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Foundation.Sync.EntryPoints;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Multivac;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Sync
{
    public sealed class StateMachine : IStateMachine
    {
        private readonly IRetryDelayService apiRetryDelayService;
        private readonly IEntryPointFactory pullSyncEntryPointFactory;
        private readonly IEntryPointFactory pushSyncEntryPointFactory;
        private readonly IEntryPointFactory retryLoopEntryPointFactory;
        private readonly int maximumNumberOfRetries;

        public StateMachine(
            IRetryDelayService apiRetryDelayService,
            IEntryPointFactory pullSyncEntryPointFactory,
            IEntryPointFactory pushSyncEntryPointFactory,
            IEntryPointFactory retryLoopEntryPointFactory,
            int maximumNumberOfRetries)
        {
            Ensure.Argument.IsNotNull(apiRetryDelayService, nameof(apiRetryDelayService));
            Ensure.Argument.IsNotNull(pullSyncEntryPointFactory, nameof(pullSyncEntryPointFactory));
            Ensure.Argument.IsNotNull(pushSyncEntryPointFactory, nameof(pushSyncEntryPointFactory));
            Ensure.Argument.IsNotNull(retryLoopEntryPointFactory, nameof(retryLoopEntryPointFactory));

            this.apiRetryDelayService = apiRetryDelayService;
            this.pullSyncEntryPointFactory = pullSyncEntryPointFactory;
            this.pushSyncEntryPointFactory = pushSyncEntryPointFactory;
            this.retryLoopEntryPointFactory = retryLoopEntryPointFactory;
            this.maximumNumberOfRetries = maximumNumberOfRetries;
        }

        public async Task Run(ISyncStateQueue queue, IObservable<bool> abort)
        {
            var retryCounter = 0;
            var repeatPeviousState = false;

            var syncState = queue.Dequeue();
            var entryPoint = chooseNextEntryPoint(syncState);

            while (entryPoint != null && await abort.FirstAsync() == false)
            {
                if (retryCounter == 0 && entryPoint is ISpeculativePreloadable preloadable)
                    preloadable.Preload();

                var lastResult = await runUntilReachingDeadEnd(entryPoint, abort);
                if (lastResult is ApiServerFailure apiServerFailure)
                {
                    if (retryCounter++ >= maximumNumberOfRetries)
                        throw apiServerFailure.Exception;

                    entryPoint = retryLoopEntryPointFactory.Create();
                    repeatPeviousState = true;
                    continue;
                }

                apiRetryDelayService.Reset();

                if (repeatPeviousState)
                    repeatPeviousState = false;
                else
                    syncState = queue.Dequeue();

                entryPoint = chooseNextEntryPoint(syncState);
            }
        }

        private IState chooseNextEntryPoint(SyncState state)
        {
            switch (state)
            {
                case Pull:
                    return pullSyncEntryPointFactory.Create();

                case Push:
                    return pushSyncEntryPointFactory.Create();

                default:
                    return null;
            }
        }

        private static async Task<IResult> runUntilReachingDeadEnd(IState entryPoint, IObservable<bool> abort)
        {
            IResult lastResult = null;
            var state = entryPoint;
            while (state != null && await abort.FirstAsync() == false)
            {
                lastResult = await state.Run();
                state = getNextState(lastResult);
            }

            return lastResult;
        }

        private static IState getNextState(IResult result)
        {
            switch (result)
            {
                case Proceed success:
                    return success.NextState;

                case ApiServerFailure _:
                    return null;

                case Error error:
                    throw error.Exception;

                default:
                    throw new ArgumentException($"Unknown result type ${result.GetType().FullName}");
            }
        }
    }
}
