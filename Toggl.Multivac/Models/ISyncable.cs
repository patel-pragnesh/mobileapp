using System;

namespace Toggl.Multivac.Models
{
    public interface ISyncable
    {
        DateTimeOffset At { get; }
    }
}
