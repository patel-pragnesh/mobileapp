using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        private readonly Func<TModel, TModel, ConflictResolutionMode> conflictResolution;

        private readonly IRivalsResolver<TModel> rivalsResolver;

        public Repository(IRealmAdapter<TModel> adapter,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution, IRivalsResolver<TModel> rivalsResolver)
            : base(adapter)
        {
            Ensure.Argument.IsNotNull(conflictResolution, nameof(conflictResolution));

            this.conflictResolution = conflictResolution;
            this.rivalsResolver = rivalsResolver;
        }

        public IObservable<TModel> Create(TModel entity)
        {
            Ensure.Argument.IsNotNull(entity, nameof(entity));

            return Observable
                .Start(() => Adapter.Create(entity))
                .Catch<TModel, Exception>(ex => Observable.Throw<TModel>(new DatabaseException(ex)));
        }
        
        public IObservable<IEnumerable<IConflictResolutionResult<TModel>>> BatchUpdate(IList<TModel> entities)
        {
            Ensure.Argument.IsNotNull(entities, nameof(entities));
            Ensure.Argument.IsNotNull(conflictResolution, nameof(conflictResolution));

            return CreateObservable(() => Adapter.BatchUpdate(entities, conflictResolution, rivalsResolver));
        }

        public IObservable<TModel> GetById(long id)
            => CreateObservable(() => Adapter.Get(id));

        public static Repository<TModel> For<TRealmEntity>(
            Func<Realms.Realm> getRealmInstance,
            Func<TModel, Realms.Realm, TRealmEntity> convertToRealm,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver = null)
            where TRealmEntity : RealmObject, TModel, IUpdatesFrom<TModel>
            => new Repository<TModel>(
                new RealmAdapter<TRealmEntity, TModel>(getRealmInstance, convertToRealm),
                conflictResolution,
                rivalsResolver);
    }
}
