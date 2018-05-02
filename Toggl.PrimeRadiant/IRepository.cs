using System;
using System.Collections.Generic;
using System.Reactive;

namespace Toggl.PrimeRadiant
{
    public interface IRepository<TModel>
    {
        IObservable<TModel> GetById(long id);
        IObservable<TModel> Create(TModel entity);
        IObservable<TModel> Update(long id, TModel entity);
        IObservable<IEnumerable<IConflictResolutionResult<TModel>>> BatchUpdate(
            IList<TModel> batch,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver);
        IObservable<Unit> Delete(long id);
        IObservable<IEnumerable<TModel>> GetAll();
        IObservable<IEnumerable<TModel>> GetAll(Func<TModel, bool> predicate);
    }
}
