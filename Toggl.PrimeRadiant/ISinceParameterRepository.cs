using System;

namespace Toggl.PrimeRadiant
{
    public interface ISinceParameterRepository
    {
        DateTimeOffset? Get(Type entityType);

        void Set(Type entityType, DateTimeOffset? since);
    }
}
