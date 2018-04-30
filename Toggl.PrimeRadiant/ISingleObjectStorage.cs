using System;
using System.Reactive;

namespace Toggl.PrimeRadiant
{
    public interface ISingleObjectStorage<TModel> : IRepository<TModel>
    {
        IObservable<TModel> Single();
        IObservable<Unit> Delete();
        IObservable<TModel> Update(TModel entity);
    }
}
