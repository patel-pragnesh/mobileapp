using System;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushOneEntityState<TModel>
        where TModel : class, IIdentifiable, IDatabaseSyncable
    {
        public StateResult<TModel> CreateEntity { get; } = new StateResult<TModel>();
        public StateResult<TModel> DeleteEntity { get; } = new StateResult<TModel>();
        public StateResult<TModel> UpdateEntity { get; } = new StateResult<TModel>();
        public StateResult<TModel> DeleteEntityLocally { get; } = new StateResult<TModel>();

        public IObservable<ITransition> Start(TModel entityToPush)
            => createObservable(entityToPush)
                .Select(entity =>
                    entity.IsDeleted
                        ? wasNotPublished(entity)
                            ? deleteLocally(entity)
                            : delete(entity)
                        : wasNotPublished(entity)
                            ? create(entity)
                            : update(entity));

        private IObservable<TModel> createObservable(TModel entity)
            => entity == null
                ? Observable.Throw<TModel>(new ArgumentNullException(nameof(entity)))
                : Observable.Return(entity);

        private bool wasNotPublished(TModel entity)
            => entity.Id < 0;

        private ITransition delete(TModel entity) => DeleteEntity.Transition(entity);

        private ITransition create(TModel entity) => CreateEntity.Transition(entity);

        private ITransition update(TModel entity) => UpdateEntity.Transition(entity);

        private ITransition deleteLocally(TModel entity) => DeleteEntityLocally.Transition(entity);
    }
}
