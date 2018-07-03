using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    public interface ITrackableEvent
    {
        string EventName { get; }
        Dictionary<string, string> ToDictionary();
    }
}
