using System;
using Toggl.Multivac.Models;
using Newtonsoft.Json;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Tag : ITag
    {
        public long Id { get; set; }

        public long WorkspaceId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        [JsonProperty("deleted_at")]
        public DateTimeOffset? ServerDeletedAt { get; set; }

        [JsonConstructor]
        public Tag(long id, long workspaceId, string name, DateTimeOffset? at, DateTimeOffset? serverDeletedAt)
        {
            Id = id;
            WorkspaceId = workspaceId;
            Name = name;
            At = at ?? DateTimeOffset.UtcNow;
            ServerDeletedAt = serverDeletedAt;
        }
    }
}
