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

        [JsonProperty("at")]
        public DateTimeOffset? DirtyAt
        {
            get => At;
            set => At = value ?? DateTimeOffset.UtcNow;
        }

        [JsonIgnore]
        public DateTimeOffset At { get; set; }

        [JsonProperty("deleted_at")]
        public DateTimeOffset? ServerDeletedAt { get; set; }
    }
}
