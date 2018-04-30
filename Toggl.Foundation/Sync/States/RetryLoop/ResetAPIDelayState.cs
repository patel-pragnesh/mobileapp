using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync.States.RetryLoop
{
    internal sealed class ResetApiDelayState : IState
    {
        private readonly IRetryDelayService delay;

        private readonly IResult continueWith;

        public ResetApiDelayState(IRetryDelayService delay, IState nextState = null)
        {
            Ensure.Argument.IsNotNull(delay, nameof(delay));

            this.delay = delay;

            continueWith = new Proceed(nextState);
        }

        public IEnumerable<IResult> AllPossibleOutcomes
            => new[] { continueWith };

        public IObservable<IResult> Run()
            => Observable.Return(continueWith)
                .Do(_ => delay.Reset());
    }
}
