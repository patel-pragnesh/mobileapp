using System;

namespace Toggl.Multivac.Models
{
    public interface IHasLastChangedDate
    {
        DateTimeOffset At { get; }
    }
}
