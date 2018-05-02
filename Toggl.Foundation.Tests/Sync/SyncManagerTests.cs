using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Xunit;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Tests.Generators;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncManagerTests
    {
        public abstract class SyncManagerTestBase
        {
            protected ISyncStateQueue Queue { get; } = Substitute.For<ISyncStateQueue>();
            protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
            protected ISyncProgressManager ProgressManager { get; } = Substitute.For<ISyncProgressManager>();
            protected IStateMachine StateMachine { get; } = Substitute.For<IStateMachine>();
            protected ISyncManager SyncManager { get; }

            protected SyncManagerTestBase()
            {
                SyncManager = new SyncManager(Queue, AnalyticsService, ProgressManager, StateMachine);
            }
        }

        public sealed class TheConstuctor : SyncManagerTestBase
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FourParameterConstructorTestData))]
            public void ThrowsIfAnyArgumentIsNull(bool useQueue, bool useProgressManager, bool useAnalyticsService, bool useStateMachine)
            {
                var queue = useQueue ? Queue : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var progressManager = useProgressManager ? ProgressManager : null;
                var stateMachine = useStateMachine ? StateMachine : null;

                // ReSharper disable once ObjectCreationAsStatement
                Action constructor = () => new SyncManager(queue, analyticsService, progressManager, stateMachine);

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
