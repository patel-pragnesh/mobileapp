using System;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BaseCreateEntityState<TModel> : BasePushEntityState<TModel>
        where TModel : class, IIdentifiable, IDatabaseSyncable
    {
        public StateResult<TModel> CreatingFinished { get; } = new StateResult<TModel>();

        public BaseCreateEntityState(ITogglApi api, IRepository<TModel> repository)
            : base(api, repository)
        {
        }

        public override IObservable<ITransition> Start(TModel entity)
            => create(entity)
                .SelectMany(Overwrite(entity))
                .Select(CreatingFinished.Transition)
                .Catch(Fail(entity));

        private IObservable<TModel> create(TModel entity)
            => entity == null
                ? Observable.Throw<TModel>(new ArgumentNullException(nameof(entity)))
                : Create(entity);

        protected abstract IObservable<TModel> Create(TModel entity);
    }
}
