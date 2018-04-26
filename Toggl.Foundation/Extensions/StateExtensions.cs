using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Results;

namespace Toggl.Foundation.Extensions
{
    public static class StateExtensions
    {
        public static async Task RunUntilReachingDeadEnd(this IState entryPoint, IObservable<Unit> abort)
        {
            if (entryPoint is ISpeculativePreloadable preloadable)
                preloadable.PreloadRecursively();

            var state = entryPoint;
            while (state != null)
            {
                var result = await state.Run(abort);
                state = getNextState(result);
            }
        }

        private static IState getNextState(IResult result)
        {
            switch (result)
            {
                case Success success:
                    return success.NextState;

                case Abort _:
                    return null;

                case Error error:
                    throw error.Exception;

                default:
                    throw new ArgumentException($"Unknown result type ${result.GetType().FullName}");
            }
        }
    }
}
