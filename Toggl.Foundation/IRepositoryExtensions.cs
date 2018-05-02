using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IObservable<IConflictResolutionResult<TModel>> UpdateWithConflictResolution<TModel>(
            this IRepository<TModel> repository,
            TModel entity,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver = null)
            where TModel : IIdentifiable, IDatabaseSyncable
            => repository
                .BatchUpdate(new List<TModel> { entity }, conflictResolution, rivalsResolver)
                .SingleAsync()
                .Select(entities => entities.First());
    }
}
