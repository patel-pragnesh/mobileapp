using System;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BaseUnsyncableEntityState<TModel> : ISyncState<(Exception Reason, TModel Entity)>
        where TModel : IBaseModel, IDatabaseSyncable
    {
        private readonly IRepository<TModel> repository;

        public StateResult<TModel> MarkedAsUnsyncable { get; } = new StateResult<TModel>();

        public BaseUnsyncableEntityState(IRepository<TModel> repository)
        {
            this.repository = repository;
        }

        public IObservable<ITransition> Start((Exception Reason, TModel Entity) failedPush)
            => failedPush.Reason == null || failedPush.Entity == null
                ? failBecauseOfNullArguments(failedPush)
                : failedPush.Reason is ApiException apiException
                    ? markAsUnsyncable(failedPush.Entity, apiException.LocalizedApiErrorMessage)
                    : failBecauseOfUnexpectedError(failedPush.Reason);

        private IObservable<ITransition> failBecauseOfNullArguments((Exception Reason, TModel Entity) failedPush)
            => Observable.Throw<Transition<TModel>>(new ArgumentNullException(
                failedPush.Reason == null
                    ? nameof(failedPush.Reason)
                    : nameof(failedPush.Entity)));

        private IObservable<ITransition> failBecauseOfUnexpectedError(Exception reason)
            => Observable.Throw<Transition<TModel>>(reason);

        private IObservable<ITransition> markAsUnsyncable(TModel entity, string reason)
            => repository
                .UpdateWithConflictResolution(entity.Id, CreateUnsyncableFrom(entity, reason), overwriteIfLocalEntityDidNotChange(entity))
                .Select(updated => updated is UpdateResult<TModel> updateResult ? updateResult.Entity : entity)
                .Select(unsyncable => MarkedAsUnsyncable.Transition(CopyFrom(unsyncable)));

        private Func<TModel, TModel, ConflictResolutionMode> overwriteIfLocalEntityDidNotChange(TModel local)
            => (currentLocal, _) => HasChanged(local, currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;

        protected abstract bool HasChanged(TModel original, TModel current);

        protected abstract TModel CreateUnsyncableFrom(TModel entity, string reason);

        protected abstract TModel CopyFrom(TModel entity);
    }
}
