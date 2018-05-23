using System;
using System.Reactive.Linq;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    public sealed class TryResolveClientErrorState<TModel> : ISyncState<(Exception Error, TModel Entity)>
        where TModel : class, IDatabaseSyncable
    {
        public StateResult UnresolvedTooManyRequests { get; } = new StateResult();
        public StateResult<(Exception, TModel)> Unresolved { get; } = new StateResult<(Exception, TModel)>();

        public IObservable<ITransition> Start((Exception Error, TModel Entity) parameter)
            => parameter.Error is ClientErrorException == false
                ? Observable.Throw<ITransition>(new ArgumentException(nameof(parameter.Error)))
                : parameter.Error is TooManyRequestsException
                    ? Observable.Return((ITransition)UnresolvedTooManyRequests.Transition())
                    : Observable.Return(Unresolved.Transition(parameter));
    }
}
