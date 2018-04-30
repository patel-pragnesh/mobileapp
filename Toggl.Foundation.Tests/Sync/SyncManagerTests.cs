using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Xunit;
using FsCheck.Xunit;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Tests.Generators;
using static Toggl.Foundation.Sync.SyncState;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncManagerTests
    {
        public abstract class SyncManagerTestBase
        {
            protected Subject<SyncResult> OrchestratorSyncComplete { get; } = new Subject<SyncResult>();
            protected Subject<SyncState> OrchestratorStates { get; } = new Subject<SyncState>();
            protected ISyncStateQueue Queue { get; } = Substitute.For<ISyncStateQueue>();
            protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
            protected ISyncManager SyncManager { get; }

            protected SyncManagerTestBase()
            {
                SyncManager = new SyncManager(Queue, AnalyticsService);
            }
        }

        public sealed class TheConstuctor : SyncManagerTestBase
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyArgumentIsNull(bool useQueue, bool useAnalyticsService)
            {
                var queue = useQueue ? Queue : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;

                // ReSharper disable once ObjectCreationAsStatement
                Action constructor = () => new SyncManager(queue, analyticsService);

                constructor.ShouldThrow<ArgumentNullException>();
            }
        }

        public abstract class ThreadSafeQueingMethodTests : SyncManagerTestBase
        {
        }

        public abstract class SyncMethodTests : ThreadSafeQueingMethodTests
        {
        }

        public sealed class ThePushSyncMethod : SyncMethodTests
        {
        }

        public sealed class TheForceFullSyncMethod : SyncMethodTests
        {
        }

        public sealed class TheFreezeMethod : SyncManagerTestBase
        {
        }

        public sealed class TheProgressObservable : SyncManagerTestBase
        {
            public static IEnumerable<object[]> ExceptionsRethrownByProgressObservableOnError()
                => new[]
                {
                    new object[] { new ClientDeprecatedException(Substitute.For<IRequest>(), Substitute.For<IResponse>()) },
                    new object[] { new ApiDeprecatedException(Substitute.For<IRequest>(), Substitute.For<IResponse>()) },
                    new object[] { new UnauthorizedException(Substitute.For<IRequest>(), Substitute.For<IResponse>()),  }
                };
        }
  
        public sealed class ErrorHandling : SyncManagerTestBase
        {
        }
    }
}
