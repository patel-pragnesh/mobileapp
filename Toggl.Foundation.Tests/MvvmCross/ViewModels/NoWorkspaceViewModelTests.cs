using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class NoWorkspaceViewModelTests
    {
        public abstract class NoWorkspaceViewModelTest : BaseViewModelTests<NoWorksaceViewModel>
        {
            protected override NoWorksaceViewModel CreateViewModel()
                => new NoWorksaceViewModel(NavigationService, DataSource);
        }

        public sealed class TheConstructor : NoWorkspaceViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useDataSource)
            {
                var navigationService = useNavigationService ? NavigationService : null;
                var dataSource = useDataSource ? DataSource : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new NoWorksaceViewModel(navigationService, dataSource);

                tryingToConstructWithEmptyParameters.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheTryAgainCommand : NoWorkspaceViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesWhenAnotherWorkspaceIsFetched()
            {
                var workspace = Substitute.For<IThreadSafeWorkspace>();
                DataSource.Workspaces.GetAll().Returns(Observable.Return(new List<IThreadSafeWorkspace>() { workspace }));

                ViewModel.TryAgainCommand.Execute();

                await NavigationService.Received().Close(Arg.Is(ViewModel));
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNothingWhenNoWorkspacesAreFetched()
            {
                var workspace = Substitute.For<IThreadSafeWorkspace>();

                ViewModel.TryAgainCommand.Execute();

                await NavigationService.DidNotReceive().Close(Arg.Is(ViewModel));
            }
        }

        public sealed class TheCreateWorkspaceCommand : NoWorkspaceViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesAfterNewWorkspaceIsCreated()
            {
                var workspace = Substitute.For<IThreadSafeWorkspace>();
                DataSource.Workspaces.Create(Arg.Any<string>()).Returns(Observable.Return(workspace));
                DataSource.Workspaces.GetAll().Returns(Observable.Return(new List<IThreadSafeWorkspace>() { workspace }));

                await ViewModel.CreateWorkspaceCommand.ExecuteAsync();

                await NavigationService.Received().Close(Arg.Is(ViewModel));
            }
        }
    }
}
