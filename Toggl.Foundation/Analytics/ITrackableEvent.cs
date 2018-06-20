using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    public interface ITrackableEvent
    {
        string EventName { get; }
        Dictionary<string, string> ToDictionary();
    }

    public static class TrackableEventExtensions 
    {
        public static void TrackWith(this ITrackableEvent trackableEvent, IAnalyticsService analyticsService)
            => analyticsService.Track(trackableEvent.EventName, trackableEvent.ToDictionary());
    }
}
