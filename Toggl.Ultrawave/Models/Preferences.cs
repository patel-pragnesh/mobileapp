using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Serialization.Converters;
using System;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Preferences : IPreferences
    {
        private const long fakeId = 0;

        [JsonIgnore]
        public long Id => fakeId;

        [JsonProperty("timeofday_format")]
        [JsonConverter(typeof(TimeFormatConverter))]
        public TimeFormat TimeOfDayFormat { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateFormat DateFormat { get; set; }

        [JsonConverter(typeof(StringEnumConverter), true)]
        public DurationFormat DurationFormat { get; set; }

        [JsonProperty("CollapseTimeEntries")]
        public bool CollapseTimeEntries { get; set; }
    }
}
