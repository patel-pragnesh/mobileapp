using System;

namespace Toggl.Foundation
{
    public interface ITimeService
    {
        DateTimeOffset CurrentDateTime { get; }

        IObservable<DateTimeOffset> CurrentDateTimeObservable { get; } 

        IObservable<DateTimeOffset> MidnightObservable { get; }
    }
}
