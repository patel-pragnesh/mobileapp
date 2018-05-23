using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States
{
    public abstract class BaseDeleteLocalEntityState<TModel> : ISyncState<TModel>
        where TModel : class
    {
        public StateResult Deleted { get; } = new StateResult();
        public StateResult DeletingFailed { get; } = new StateResult();

        protected IRepository<TModel> Repository { get; }

        public BaseDeleteLocalEntityState(IRepository<TModel> repository)
        {
            Repository = repository;
        }

        public IObservable<ITransition> Start(TModel entity)
            => delete(entity)
                .Select(_ => Deleted.Transition())
                .Catch((Exception e) => Observable.Return(DeletingFailed.Transition()));

        private IObservable<Unit> delete(TModel entity)
            => entity == null
                ? Observable.Throw<Unit>(new ArgumentNullException(nameof(entity)))
                : Delete(entity);

        protected abstract IObservable<Unit> Delete(TModel entity);
    }
}
