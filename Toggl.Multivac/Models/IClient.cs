using System;

namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }

        DateTimeOffset At { get; }
    }
}
