using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    public abstract class BaseDeleteEntityState<TModel> : BasePushEntityState<TModel>
        where TModel : class, IIdentifiable, IDatabaseSyncable
    {
        public StateResult DeletingFinished { get; } = new StateResult();

        public BaseDeleteEntityState(ITogglApi api, IRepository<TModel> repository)
            : base(api, repository)
        {
        }

        public override IObservable<ITransition> Start(TModel entity)
            => delete(entity)
                .SelectMany(_ => Repository.Delete(entity.Id))
                .Select(_ => DeletingFinished.Transition())
                .Catch(Fail(entity));

        private IObservable<Unit> delete(TModel entity)
            => entity == null
                ? Observable.Throw<Unit>(new ArgumentNullException(nameof(entity)))
                : Delete(entity);

        protected abstract IObservable<Unit> Delete(TModel entity);
    }
}
