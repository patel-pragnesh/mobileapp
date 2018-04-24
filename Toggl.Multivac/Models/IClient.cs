using System;

namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable, ISyncable, IDeletable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
