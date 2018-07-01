using System;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.Tests.Sync
{
    public interface ITestAnalyticsService : IAnalyticsService
    {
        IAnalyticsEvent<string> TestEvent { get; }
    }
}
