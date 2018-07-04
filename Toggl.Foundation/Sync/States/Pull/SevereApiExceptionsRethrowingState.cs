using System;
using System.Reactive.Linq;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class SevereApiExceptionsRethrowingState : ISyncState<ApiException>
    {
        public StateResult<ApiException> Retry { get; } = new StateResult<ApiException>();

        public IObservable<ITransition> Start(ApiException exception)
            => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(Retry.Transition(exception));

        private bool shouldRethrow(ApiException e)
            => !(e is ServerErrorException || e is ClientErrorException)
               || e is ApiDeprecatedException
               || e is ClientDeprecatedException
               || e is UnauthorizedException;
    }
}
