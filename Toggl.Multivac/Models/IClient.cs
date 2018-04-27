using System;

namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable, ILastChangeDatable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
