using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Realms;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Exceptions;

namespace Toggl.PrimeRadiant.Realm
{
    internal sealed class Repository<TModel> : BaseStorage<TModel>, IRepository<TModel>
        where TModel : IIdentifiable
    {
        public Repository(IRealmAdapter<TModel> adapter)
            : base(adapter)
        {
        }

        public IObservable<TModel> Create(TModel entity)
        {
            Ensure.Argument.IsNotNull(entity, nameof(entity));

            return Observable
                .Start(() => Adapter.Create(entity))
                .Catch<TModel, Exception>(ex => Observable.Throw<TModel>(new DatabaseException(ex)));
        }

        public IObservable<IEnumerable<IConflictResolutionResult<TModel>>> BatchUpdate(
            IList<TModel> batch,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver)
        {
            Ensure.Argument.IsNotNull(batch, nameof(batch));
            Ensure.Argument.IsNotNull(conflictResolution, nameof(conflictResolution));

            return CreateObservable(() => Adapter.BatchUpdate(batch, conflictResolution, rivalsResolver));
        }

        public IObservable<TModel> GetById(long id)
            => CreateObservable(() => Adapter.Get(id));

        public static Repository<TModel> For<TRealmEntity>(
            Func<Realms.Realm> getRealmInstance,
            Func<TModel, Realms.Realm, TRealmEntity> convertToRealm)
            where TRealmEntity : RealmObject, TModel, IUpdatesFrom<TModel>
            => new Repository<TModel>(new RealmAdapter<TRealmEntity, TModel>(getRealmInstance, convertToRealm));
    }
}
