using System;

namespace Toggl.Multivac.Models
{
    public interface ITag : IIdentifiable, ISyncable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
