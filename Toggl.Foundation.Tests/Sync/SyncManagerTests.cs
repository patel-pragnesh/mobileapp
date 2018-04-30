using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Xunit;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Tests.Generators;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncManagerTests
    {
        public abstract class SyncManagerTestBase
        {
            protected ISyncStateQueue Queue { get; } = Substitute.For<ISyncStateQueue>();
            protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
            protected ISyncProgressManager ProgressManager { get; } = Substitute.For<ISyncProgressManager>();
            protected ISyncManager SyncManager { get; }

            protected SyncManagerTestBase()
            {
                SyncManager = new SyncManager(Queue, AnalyticsService, ProgressManager, null, null);
            }
        }

        public sealed class TheConstuctor : SyncManagerTestBase
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyArgumentIsNull(bool useQueue, bool useProgressManager, bool useAnalyticsService)
            {
                var queue = useQueue ? Queue : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var progressManager = useProgressManager ? ProgressManager : null;

                // ReSharper disable once ObjectCreationAsStatement
                Action constructor = () => new SyncManager(queue, analyticsService, progressManager, null, null);

                constructor.ShouldThrow<ArgumentNullException>();
            }
        }
    }
}
