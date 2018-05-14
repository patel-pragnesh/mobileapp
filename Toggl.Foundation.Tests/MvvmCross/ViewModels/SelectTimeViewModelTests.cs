using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectTimeViewModelTests
    {
        public abstract class SelectTimeViewModelTest : BaseViewModelTests<SelectTimeViewModel>
        {
            protected override SelectTimeViewModel CreateViewModel()
                => new SelectTimeViewModel(NavigationService, TimeService);
        }

        public sealed class TheIncreaseDurationCommand : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IncreasesTheStartTimeWhenIsRunning()
            {
                var minutes = 5;
                var startTime = DateTimeOffset.Now - TimeSpan.FromHours(10);

                ViewModel.StartTime = startTime;
                ViewModel.StopTime = null;

                var duration = ViewModel.Duration;

                ViewModel.IncreaseDurationCommand.Execute(minutes);

                ViewModel.StartTime.Should().Be(startTime - TimeSpan.FromMinutes(minutes));
                ViewModel.StopTime.Should().Be(null);
                ViewModel.Duration.Should().Be(duration + TimeSpan.FromMinutes(minutes));
            }

            [Fact, LogIfTooSlow]
            public void IncreasesTheStopTimeWhenIsNotRunning()
            {
                var minutes = 5;
                var startTime = DateTimeOffset.Now;
                var stopTime = DateTimeOffset.Now + TimeSpan.FromHours(1);

                ViewModel.StartTime = startTime;
                ViewModel.StopTime = stopTime;

                var duration = ViewModel.Duration;

                ViewModel.IncreaseDurationCommand.Execute(minutes);

                ViewModel.StartTime.Should().Be(startTime);
                ViewModel.StopTime.Should().Be(stopTime + TimeSpan.FromMinutes(minutes));
                ViewModel.Duration.Should().Be(duration + TimeSpan.FromMinutes(minutes));
            }

            [Theory, LogIfTooSlow]
            [InlineData(5)]
            [InlineData(10)]
            [InlineData(30)]
            public void IncreasesTheDurationForCorrectAmountOfTime(int minutes)
            {
                var startTime = DateTimeOffset.Now;
                var stopTime = DateTimeOffset.Now + TimeSpan.FromHours(1);

                ViewModel.StartTime = startTime;
                ViewModel.StopTime = stopTime;

                var duration = ViewModel.Duration;

                ViewModel.IncreaseDurationCommand.Execute(minutes);

                ViewModel.Duration.Should().Be(duration + TimeSpan.FromMinutes(minutes));
            }
        }

        public sealed class TheConstructor : SelectTimeViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useTimeService)
            {
                var navigationService = useNavigationService ? NavigationService : null;
                var timeService = useTimeService ? TimeService : null;

                Action constructingWithEmptyParameters =
                    () => new SelectTimeViewModel(navigationService, timeService);

                constructingWithEmptyParameters.ShouldThrow<ArgumentNullException>();
            }
        }


    }
}
