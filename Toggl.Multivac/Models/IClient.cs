using System;

namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable
    {
        long WorkspaceId { get; }

        string Name { get; }

        DateTimeOffset At { get; }

        DateTimeOffset? ServerDeletedAt { get; }
    }
}
