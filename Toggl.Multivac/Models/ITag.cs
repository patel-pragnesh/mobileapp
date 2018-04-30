using System;

namespace Toggl.Multivac.Models
{
    public interface ITag : IIdentifiable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }

        DateTimeOffset At { get; }
    }
}
