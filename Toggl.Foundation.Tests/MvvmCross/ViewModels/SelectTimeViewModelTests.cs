using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Helper;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectTimeViewModelTests
    {
        public abstract class SelectTimeViewModelTest : BaseViewModelTests<SelectTimeViewModel>
        {
            protected override SelectTimeViewModel CreateViewModel()
                => new SelectTimeViewModel(NavigationService, TimeService);

            protected SelectTimeParameters CreateParameter(DateTimeOffset start, DateTimeOffset? stop)
            {
                var dateFormat = DateFormat.FromLocalizedDateFormat("MM.DD.YYYY");
                var timeFormat = TimeFormat.FromLocalizedTimeFormat("H:mm");

                return SelectTimeParameters
                    .CreateFromBindingString("StartTime", start, stop)
                    .WithFormats(dateFormat, timeFormat);
            }

            public SelectTimeParameters Parameters { get; protected set; }
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

        public sealed class TheMinStartTimeProperty : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForStoppedEntry()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now + TimeSpan.FromHours(1);
                var parameter = CreateParameter(start, stop);

                ViewModel.Prepare(parameter);

                ViewModel.MinStartTime.Should().Be(stop - Constants.MaxTimeEntryDuration);
            }

            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForRunningEntry()
            {
                var start = DateTimeOffset.Now;
                var parameter = CreateParameter(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.MinStartTime.Should().Be(Constants.EarliestAllowedStartTime);
            }

            [Fact, LogIfTooSlow]
            public void ChangingStartAndStopTimeDoesNotChangeBoundaryBeforePrepareHasRun()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now.AddHours(1);
                var nextStop = DateTimeOffset.Now.AddHours(1);
                var oldMinStartTime = ViewModel.MinStartTime;

                ViewModel.StartTime = start;
                ViewModel.StopTime = stop;

                ViewModel.MinStartTime.Should().Be(oldMinStartTime);
            }
        }

        public sealed class TheMaxStartTimeProperty : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForStoppedEntry()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now + TimeSpan.FromHours(1);
                var parameter = CreateParameter(start, stop);

                ViewModel.Prepare(parameter);

                ViewModel.MaxStartTime.Should().Be(stop);
            }

            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForRunningEntry()
            {
                var start = DateTimeOffset.Now;
                var parameter = CreateParameter(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.MaxStartTime.Should().Be(Constants.LatestAllowedStartTime);
            }

            [Fact, LogIfTooSlow]
            public void ChangingStartAndStopTimeDoesNotChangeBoundaryBeforePrepareHasRun()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now.AddHours(1);
                var nextStop = DateTimeOffset.Now.AddHours(1);
                var oldMaxStartTime = ViewModel.MaxStartTime;

                ViewModel.StartTime = start;
                ViewModel.StopTime = stop;

                ViewModel.MaxStartTime.Should().Be(oldMaxStartTime);
            }
        }

        public sealed class TheMinStopTimeProperty : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForStoppedEntry()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now + TimeSpan.FromHours(1);
                var parameter = CreateParameter(start, stop);

                ViewModel.Prepare(parameter);

                ViewModel.MinStopTime.Should().Be(start);
            }

            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForRunningEntry()
            {
                var start = DateTimeOffset.Now;
                var parameter = CreateParameter(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.MinStopTime.Should().Be(start);
            }

            [Fact, LogIfTooSlow]
            public void ChangingStartAndStopTimeDoesNotChangeBoundaryBeforePrepareHasRun()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now.AddHours(1);
                var nextStop = DateTimeOffset.Now.AddHours(1);
                var oldMinStopTime = ViewModel.MinStopTime;

                ViewModel.StartTime = start;
                ViewModel.StopTime = stop;

                ViewModel.MinStopTime.Should().Be(oldMinStopTime);
            }
        }

        public sealed class TheMaxStopTimeProperty : SelectTimeViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForStoppedEntry()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now + TimeSpan.FromHours(1);
                var parameter = CreateParameter(start, stop);

                ViewModel.Prepare(parameter);

                ViewModel.MaxStopTime.Should().Be(start + Constants.MaxTimeEntryDuration);
            }

            [Fact, LogIfTooSlow]
            public void IsCorrectlyInitializedOnPrepareForRunningEntry()
            {
                var start = DateTimeOffset.Now;
                var parameter = CreateParameter(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.MaxStopTime.Should().Be(start + Constants.MaxTimeEntryDuration);
            }

            [Fact, LogIfTooSlow]
            public void ChangingStartAndStopTimeDoesNotChangeBoundaryBeforePrepareHasRun()
            {
                var start = DateTimeOffset.Now;
                var stop = DateTimeOffset.Now.AddHours(1);
                var nextStop = DateTimeOffset.Now.AddHours(1);
                var oldMaxStopTime = ViewModel.MaxStopTime;

                ViewModel.StartTime = start;
                ViewModel.StopTime = stop;

                ViewModel.MaxStopTime.Should().Be(oldMaxStopTime);
            }
        }
    }
}
