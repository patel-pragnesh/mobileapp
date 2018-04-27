using System;

namespace Toggl.Multivac.Models
{
    public interface ITag : IIdentifiable, ILastChangeDatable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
