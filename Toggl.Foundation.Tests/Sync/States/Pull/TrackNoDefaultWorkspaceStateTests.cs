using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class TrackNoDefaultWorkspaceStateTests
    {
        private readonly IFetchObservables fetchObservables = Substitute.For<IFetchObservables>();

        private readonly IPersistState internalState = Substitute.For<IPersistState>();

        private readonly IAnalyticsService analyticsService = Substitute.For<IAnalyticsService>();

        private readonly TrackNoDefaultWorkspaceState state;

        public TrackNoDefaultWorkspaceStateTests()
        {
            state = new TrackNoDefaultWorkspaceState(internalState, analyticsService);
        }

        [Fact]
        public async Task TracksTheEventWhenUserDoesNotHaveDefaultWorkspace()
        {
            var user = new MockUser();
            user.DefaultWorkspaceId = null;

            fetchObservables.GetSingle<IUser>().Returns(Observable.Return<IUser>(user));
            var transition = await state.Start(fetchObservables);

            analyticsService.NoDefaultWorkspace.Received().Track();
        }

        [Fact]
        public async Task DoesNotTrackAnEventWhenUserHasDefaultWorkspace()
        {
            var user = new MockUser();
            user.DefaultWorkspaceId = 123;

            fetchObservables.GetSingle<IUser>().Returns(Observable.Return<IUser>(user));
            var transition = await state.Start(fetchObservables);

            analyticsService.NoDefaultWorkspace.DidNotReceive().Track();
        }
    }
}
