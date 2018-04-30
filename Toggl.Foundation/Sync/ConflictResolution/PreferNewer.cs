using Toggl.Foundation.Sync.ConflictResolution.Selectors;
using Toggl.PrimeRadiant;
using Toggl.Multivac;
using static Toggl.PrimeRadiant.ConflictResolutionMode;
using System;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    internal sealed class PreferNewer<T> : IConflictResolver<T>
        where T : class
    {
        public TimeSpan MarginOfError { get; }

        private ISyncSelector<T> selector { get; }

        public PreferNewer(ISyncSelector<T> selector)
            : this(selector, TimeSpan.Zero)
        {
        }

        public PreferNewer(ISyncSelector<T> selector, TimeSpan marginOfError)
        {
            Ensure.Argument.IsNotNull(marginOfError, nameof(marginOfError));
            Ensure.Argument.IsNotNull(selector, nameof(selector));

            this.MarginOfError = marginOfError;
            this.selector = selector;
        }

        public ConflictResolutionMode Resolve(T localEntity, T serverEntity)
        {
            Ensure.Argument.IsNotNull(serverEntity, nameof(serverEntity));

            if (serverEntity is IDeletable deletable && deletable.ServerDeletedAt.HasValue)
                return localEntity == null ? Ignore : Delete;

            if (localEntity == null)
                return Create;

            if (selector.IsInSync(localEntity))
                return Update;

            var receivedDataIsOutdated = selector.LastModified(localEntity) > selector.LastModified(serverEntity).Subtract(MarginOfError);
            if (receivedDataIsOutdated)
                return Ignore;

            return Update;
        }
    }
}
