using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BasePushState<TModel>
        where TModel : class, IBaseModel, IDatabaseSyncable
    {
        private readonly IRepository<TModel> repository;

        public StateResult<TModel> PushEntity { get; } = new StateResult<TModel>();
        public StateResult NothingToPush { get; } = new StateResult();

        public BasePushState(IRepository<TModel> repository)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));
        
            this.repository = repository;
        }

        public IObservable<ITransition> Start() =>
            getOldestUnsynced()
                .SingleAsync()
                .Select(entity =>
                    entity != null
                        ? (ITransition)PushEntity.Transition(CopyFrom(entity))
                        : NothingToPush.Transition());

        private IObservable<TModel> getOldestUnsynced()
            => repository
                .GetAll(syncNeeded)
                .SingleAsync()
                .Select(entities => entities
                    .OrderBy(LastChange)
                    .FirstOrDefault());

        protected abstract DateTimeOffset LastChange(TModel entity);

        private bool syncNeeded(TModel entity)
            => entity.SyncStatus == SyncStatus.SyncNeeded;
            
        protected abstract TModel CopyFrom(TModel entity);
    }
}
