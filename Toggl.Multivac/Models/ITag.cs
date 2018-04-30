using System;

namespace Toggl.Multivac.Models
{
    public interface ITag : IIdentifiable
    {
        long WorkspaceId { get; }

        string Name { get; }

        DateTimeOffset At { get; }

        DateTimeOffset? DeletedAt { get; }
    }
}
