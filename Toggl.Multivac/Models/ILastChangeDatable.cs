using System;

namespace Toggl.Multivac.Models
{
    public interface ILastChangeDatable
    {
        DateTimeOffset At { get; }
    }
}
