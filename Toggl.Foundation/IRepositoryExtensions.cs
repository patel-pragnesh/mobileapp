using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation
{
    public static class IRepositoryExtensions
    {
        public static IObservable<TModel> Update<TModel>(this IRepository<TModel> repository, TModel entity)
            where TModel : IIdentifiable, IDatabaseSyncable
            => repository.Update(entity.Id, entity);

        public static IObservable<IEnumerable<IConflictResolutionResult<TModel>>> UpdateWithConflictResolution<TModel>(
            this IRepository<TModel> repository,
            long id,
            TModel entity,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver = null)
            => repository
                .BatchUpdate(new[] { (id, entity) }, conflictResolution, rivalsResolver)
                .SingleAsync();
    }
}
