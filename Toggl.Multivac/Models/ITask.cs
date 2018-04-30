using System;

namespace Toggl.Multivac.Models
{
    public interface ITask : IIdentifiable
    {
        string Name { get; }

        long ProjectId { get; }

        long WorkspaceId { get; }

        long? UserId { get; }

        long EstimatedSeconds { get; }

        bool Active { get; }

        DateTimeOffset At { get; }

        long TrackedSeconds { get; }
    }
}
