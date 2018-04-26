using System;

namespace Toggl.Multivac.Models
{
    public interface ITag : IIdentifiable, IHasLastChangedDate, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
