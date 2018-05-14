using System;
using FluentAssertions;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels;
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

        public sealed class TheIncreaseDuration5MinCommand : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IncreasesTheStartTimeWhenIsRunning()
            {
                var minutes = 5;
                var startTime = DateTimeOffset.Now - TimeSpan.FromHours(10);

                ViewModel.StartTime = startTime;
                ViewModel.StopTime = null;

                var duration = ViewModel.Duration;

                ViewModel.IncreaseDuration5MinCommand.Execute();

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

                ViewModel.IncreaseDuration5MinCommand.Execute();

                ViewModel.StartTime.Should().Be(startTime);
                ViewModel.StopTime.Should().Be(stopTime + TimeSpan.FromMinutes(minutes));
                ViewModel.Duration.Should().Be(duration + TimeSpan.FromMinutes(minutes));
            }
        }
    }
}
