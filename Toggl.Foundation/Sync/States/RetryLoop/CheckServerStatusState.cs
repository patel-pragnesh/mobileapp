using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.RetryLoop
{
    internal sealed class CheckServerStatusState : IState
    {
        private readonly ITogglApi api;
        private readonly IScheduler scheduler;
        private readonly IRetryDelayService apiDelay;
        private readonly IRetryDelayService statusDelay;
        private readonly IObservable<Unit> delayCancellation;

        private readonly IResult retry;
        private readonly IResult serverIsAvailable;

        public IEnumerable<IResult> AllPossibleOutcomes
            => new[] { serverIsAvailable, retry };

        public CheckServerStatusState(
            ITogglApi api,
            IScheduler scheduler,
            IRetryDelayService apiDelay,
            IRetryDelayService statusDelay,
            IObservable<Unit> delayCancellation,
            IState serverIsAvailableState)
        {
            this.api = api;
            this.scheduler = scheduler;
            this.apiDelay = apiDelay;
            this.statusDelay = statusDelay;
            this.delayCancellation = delayCancellation;

            serverIsAvailable = new Proceed(serverIsAvailableState);
            retry = new Proceed(this);
        }

        public IObservable<IResult> Run()
            => delay(api.Status.IsAvailable())
                .Do(_ => statusDelay.Reset())
                .Select(_ => serverIsAvailable)
                .Catch((Exception e) => delayedRetry(getDelay(e)));

        private IObservable<IResult> delayedRetry(TimeSpan period)
            => Observable.Return(Unit.Default)
                .Delay(period, scheduler)
                .Merge(delayCancellation)
                .Select(_ => retry)
                .FirstAsync();

        private IObservable<Unit> delay(IObservable<Unit> observable)
            => observable
                .Delay(apiDelay.NextSlowDelay(), scheduler)
                .Merge(delayCancellation)
                .FirstAsync();

        private TimeSpan getDelay(Exception exception)
            => exception is InternalServerErrorException
                ? statusDelay.NextSlowDelay()
                : statusDelay.NextFastDelay();
    }
}
