using System;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BaseUpdateEntityState<TModel> : BasePushEntityState<TModel>
        where TModel : class, IBaseModel, IDatabaseSyncable
    {
        public StateResult<TModel> EntityChanged { get; } = new StateResult<TModel>();
        public StateResult<TModel> UpdatingSucceeded { get; } = new StateResult<TModel>();

        public BaseUpdateEntityState(ITogglApi api, IRepository<TModel> repository)
            : base(api, repository)
        {
        }

        public override IObservable<ITransition> Start(TModel entity)
            => update(entity)
                .SelectMany(tryOverwrite(entity))
                .SelectMany(result => result is IgnoreResult<TModel>
                    ? entityChanged(entity)
                    : succeeded(copyFrom(result)))
                .Catch(Fail(entity));

        private IObservable<TModel> update(TModel entity)
            => entity == null
                ? Observable.Throw<TModel>(new ArgumentNullException(nameof(entity)))
                : Update(entity);

        private IObservable<ITransition> entityChanged(TModel entity)
            => Observable.Return(EntityChanged.Transition(entity));

        private Func<TModel, IObservable<IConflictResolutionResult<TModel>>> tryOverwrite(TModel entity)
            => updatedEntity => Repository.UpdateWithConflictResolution(entity.Id, updatedEntity, overwriteIfLocalEntityDidNotChange(entity));

        private Func<TModel, TModel, ConflictResolutionMode> overwriteIfLocalEntityDidNotChange(TModel local)
            => (currentLocal, _) => HasChanged(local, currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;

        private IObservable<ITransition> succeeded(TModel entity)
            => Observable.Return((ITransition)UpdatingSucceeded.Transition(entity));

        private TModel copyFrom(IConflictResolutionResult<TModel> result)
        {
            switch (result)
            {
                case CreateResult<TModel> c:
                    return CopyFrom(c.Entity);
                case UpdateResult<TModel> u:
                    return CopyFrom(u.Entity);
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        protected abstract bool HasChanged(TModel original, TModel updated);

        protected abstract IObservable<TModel> Update(TModel entity);
    }
}
