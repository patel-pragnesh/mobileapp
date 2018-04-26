using System;

namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable, IHasLastChangedDate, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
