using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Xunit;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class NoWorkspaceExceptionsThrowingStateTests
    {
        private readonly IFetchObservables fetchObservables = Substitute.For<IFetchObservables>();

        private readonly NoWorkspaceDetectingState state;

        public NoWorkspaceExceptionsThrowingStateTests()
        {
            state = new NoWorkspaceDetectingState();
        }

        [Fact]
        public async Task ReturnsSuccessResultWhenWorkspacesArePresent()
        {
            var arrayWithWorkspace = new List<IWorkspace>(new[] { new MockWorkspace() });
            fetchObservables.GetList<IWorkspace>().Returns(Observable.Return(arrayWithWorkspace));

            var transition = await state.Start(fetchObservables);

            transition.Result.Should().Be(state.Continue);
        }

        [Fact, LogIfTooSlow]
        public void ThrowsExceptionsWhenNoWorkspacesAreAvailable()
        {
            var arrayWithNoWorkspace = new List<IWorkspace>();
            fetchObservables.GetList<IWorkspace>().Returns(Observable.Return<List<IWorkspace>>(arrayWithNoWorkspace));

            Func<Task> fetchWorkspaces = async () => await state.Start(fetchObservables);

            fetchWorkspaces.Should().Throw<NoWorkspaceException>();
        }

        [Fact, LogIfTooSlow]
        public void ThrowsWhenTheDeviceIsOffline()
        {
            fetchObservables.GetList<IWorkspace>().Returns(Observable.Throw<List<IWorkspace>>(new OfflineException(new Exception())));

            Action startingState = () => state.Start(fetchObservables).Wait();

            startingState.Should().Throw<OfflineException>();
        }
    }
}
