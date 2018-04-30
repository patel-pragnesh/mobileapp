using System;
using System.Reactive;
using System.Reactive.Concurrency;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.RetryLoop;
using Toggl.Multivac;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.EntryPoints
{
    public sealed class RetryLoopEntryPointFactory : IEntryPointFactory
    {
        private readonly IRetryDelayService apiDelay;
        private readonly ITogglApi api;
        private readonly IScheduler scheduler;
        private readonly IRetryDelayService statusDelay;
        private readonly IObservable<Unit> delayCancellation;

        public RetryLoopEntryPointFactory(
            ITogglApi api,
            IScheduler scheduler,
            IRetryDelayService apiDelay,
            IRetryDelayService statusDelay,
            IObservable<Unit> delayCancellation)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(scheduler, nameof(scheduler));
            Ensure.Argument.IsNotNull(apiDelay, nameof(apiDelay));
            Ensure.Argument.IsNotNull(statusDelay, nameof(statusDelay));
            Ensure.Argument.IsNotNull(delayCancellation, nameof(delayCancellation));

            this.api = api; 
            this.scheduler = scheduler;
            this.apiDelay = apiDelay;
            this.statusDelay = statusDelay;
            this.delayCancellation = delayCancellation;
        }

        public IState Create()
        {
            var finishState = new ResetApiDelayState(apiDelay);
            return new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation, finishState);
        }
    }
}
