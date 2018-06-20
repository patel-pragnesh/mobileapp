﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class ReportsViewModelTests
    {
        public abstract class ReportsViewModelTest : BaseViewModelTests<ReportsViewModel>
        {
            protected const long WorkspaceId = 10;

            protected IReportsProvider ReportsProvider { get; } = Substitute.For<IReportsProvider>();

            protected override ReportsViewModel CreateViewModel()
            {
                DataSource.ReportsProvider.Returns(ReportsProvider);
                return new ReportsViewModel(DataSource, TimeService, NavigationService, InteractorFactory, AnalyticsService);
            }

            protected async Task Initialize()
            {
                using (var block = new AutoResetEvent(false))
                {
                    NavigationService
                        .When(service => service.Navigate(Arg.Any<ReportsCalendarViewModel>()))
                        .Do(async callInfo =>
                        {
                            var calendarViewModel = callInfo.Arg<ReportsCalendarViewModel>();
                            calendarViewModel.Prepare();
                            await calendarViewModel.Initialize();
                            block.Set();
                        });

                    ViewModel.Prepare(WorkspaceId);
                    await ViewModel.Initialize();
                    ViewModel.ViewAppeared();

                    block.WaitOne();
                }
            }
        }

        public sealed class TheConstructor : ReportsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource,
                                                        bool useTimeService,
                                                        bool useNavigationService,
                                                        bool useAnalyticsService,
                                                        bool useInteractorFactory)
            {
                var timeService = useTimeService ? TimeService : null;
                var reportsProvider = useDataSource ? DataSource : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new ReportsViewModel(reportsProvider, timeService, navigationService, interactorFactory, analyticsService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : ReportsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsSegmentsJustOnceWhenChangingDateRange()
            {
                var segments = new ChartSegment[2] {
                    new ChartSegment("Project 1", "Client 1", 50f, 10, 0, "ff0000"),
                    new ChartSegment("Project 2", "Client 2", 50f, 10, 0, "00ff00")
                };
                var projectsNotSyncedCount = 0;

                var currentDate = new DateTimeOffset(2018, 5, 23, 0, 0, 0, TimeSpan.Zero);
                var start = new DateTimeOffset(2018, 5, 1, 0, 0, 0, TimeSpan.Zero);
                var end = new DateTimeOffset(2018, 5, 7, 0, 0, 0, TimeSpan.Zero);
                TimeService.CurrentDateTime.Returns(currentDate);

                var delayed = Observable
                    .Return(new ProjectSummaryReport(segments, projectsNotSyncedCount))
                    .Delay(TimeSpan.FromMilliseconds(100));

                var instant = Observable
                    .Return(new ProjectSummaryReport(segments, projectsNotSyncedCount));

                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                               .Returns(delayed, instant);

                await Initialize();
                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end));

                await delayed;
                ViewModel.Segments.Count.Should().Be(segments.Length);
            }

            [Fact, LogIfTooSlow]
            public async Task TracksAnEventWhenReportLoadsSuccessfully()
            {
                var startDateRange = new DateTimeOffset(2018, 05, 05, 0, 0, 0, TimeSpan.Zero);
                var endDateRange = startDateRange.AddDays(7);

                var totalDays = (int)(endDateRange - startDateRange).TotalDays;
                var projectsNotSyncedCount = 0;
                var loadingDuration = TimeSpan.FromSeconds(5);
                var now = new DateTimeOffset(2018, 01, 01, 0, 0, 0, TimeSpan.Zero);

                TimeService.CurrentDateTime.Returns(_ =>
                {
                    now = now + loadingDuration;
                    return now;
                });

                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                        .Returns(Observable.Return(new ProjectSummaryReport(new ChartSegment[0], projectsNotSyncedCount)));

                await Initialize();

                AnalyticsService.Received().ReportsSuccess.Track(ReportsSource.Initial, totalDays, projectsNotSyncedCount, loadingDuration.TotalMilliseconds);
            }

            [Fact, LogIfTooSlow]
            public async Task TracksAnEventWhenReportFailsToLoad()
            {
                var startDateRange = new DateTimeOffset(2018, 05, 05, 0, 0, 0, TimeSpan.Zero);
                var endDateRange = startDateRange.AddDays(7);

                var totalDays = (int)(endDateRange - startDateRange).TotalDays;
                var loadingDuration = TimeSpan.FromSeconds(5);
                var now = new DateTimeOffset(2018, 01, 01, 0, 0, 0, TimeSpan.Zero);

                TimeService.CurrentDateTime.Returns(_ =>
                {
                    now = now + loadingDuration;
                    return now;
                });

                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                        .Returns(Observable.Throw<ProjectSummaryReport>(new Exception()));

                await Initialize();

                AnalyticsService.Received().ReportsFailure.Track(ReportsSource.Initial, totalDays, loadingDuration.TotalMilliseconds);
            }
        }

        public sealed class TheBillablePercentageMethod : ReportsViewModelTest
        {
            [Property(MaxTest = 1)]
            public void IsSetToNullIfTheTotalTimeOfAReportIsZero(DateTimeOffset now)
            {
                var date = now.Date;
                var projectsNotSyncedCount = 0;
                TimeService.CurrentDateTime.Returns(now);
                var expectedStartDate = date.AddDays(1 - (int)date.DayOfWeek);
                ReportsProvider.GetProjectSummary(
                    WorkspaceId, expectedStartDate, expectedStartDate.AddDays(6))
                        .Returns(Observable.Return(new ProjectSummaryReport(new ChartSegment[0], projectsNotSyncedCount)));
                ViewModel.Prepare(WorkspaceId);

                ViewModel.Initialize().Wait();

                ViewModel.BillablePercentage.Should().BeNull();
            }
        }

        public sealed class TheIsLoadingProperty : ReportsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task IsSetToTrueWhenTheViewIsInitializedBeforeAnyLoadingOfReportsStarts()
            {
                ViewModel.Prepare(WorkspaceId);
                await ViewModel.Initialize();

                ViewModel.IsLoading.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task IsSetToTrueWhenAReportIsLoading()
            {
                var now = DateTimeOffset.Now;
                var projectsNotSyncedCount = 0;
                TimeService.CurrentDateTime.Returns(now);
                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Never<ProjectSummaryReport>());

                await Initialize();

                ViewModel.IsLoading.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task IsSetToFalseWhenLoadingIsCompleted()
            {
                var now = DateTimeOffset.Now;
                var projectsNotSyncedCount = 0;
                TimeService.CurrentDateTime.Returns(now);
                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Return(new ProjectSummaryReport(new ChartSegment[0], projectsNotSyncedCount)));

                await Initialize();

                ViewModel.IsLoading.Should().BeFalse();
            }
        }

        public sealed class TheCurrentDateRangeStringProperty : ReportsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task IsInitializedToEmptyOrNull()
            {
                ViewModel.Prepare(WorkspaceId);
                await ViewModel.Initialize();

                ViewModel.CurrentDateRangeString.Should().BeNullOrEmpty();
            }

            [Theory, LogIfTooSlow]
            [InlineData(
                2017, 12, 6,
                2017, 12, 4,
                2017, 12, 10
            )]
            [InlineData(
                2018, 2, 28,
                2018, 2, 26,
                2018, 3, 4
            )]
            [InlineData(
                2016, 4, 20,
                2016, 4, 18,
                2016, 4, 24
            )]
            [InlineData(
                2016, 12, 28,
                2016, 12, 26,
                2017, 1, 1
            )]
            public void ReturnsThisWeekWhenStartAndEndOfCurrentWeekAreSeleted(
                int currentYear, int currentMonth, int currentDay,
                int startYear, int startMonth, int startDay,
                int endYear, int endMonth, int endDay)
            {
                var currentDate = new DateTimeOffset(currentYear, currentMonth, currentDay, 0, 0, 0, TimeSpan.Zero);
                var start = new DateTimeOffset(startYear, startMonth, startDay, 0, 0, 0, TimeSpan.Zero);
                var end = new DateTimeOffset(endYear, endMonth, endDay, 0, 0, 0, TimeSpan.Zero);
                TimeService.CurrentDateTime.Returns(currentDate);
                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end).WithSource(ReportsSource.Calendar));

                ViewModel.CurrentDateRangeString.Should().Be($"{Resources.ThisWeek} ▾");
            }

            [Theory]
            [MemberData(nameof(DateRangeFormattingTestData))]
            public async Task ReturnsSelectedDateRangeAsStringIfTheSelectedPeriodIsNotTheCurrentWeek(
                DateTimeOffset start,
                DateTimeOffset end,
                DateFormat dateFormat,
                string expectedResult)
            {
                TimeService.CurrentDateTime.Returns(DateTimeOffset.UtcNow);
                var preferences = Substitute.For<IThreadSafePreferences>();
                preferences.DateFormat.Returns(dateFormat);
                var preferencesSubject = new Subject<IThreadSafePreferences>();
                DataSource.Preferences.Current.Returns(preferencesSubject.AsObservable());
                ViewModel.Prepare(0);
                await ViewModel.Initialize();
                preferencesSubject.OnNext(preferences);

                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end).WithSource(ReportsSource.Calendar));

                ViewModel.CurrentDateRangeString.Should().Be(expectedResult);
            }

            public static IEnumerable<object[]> DateRangeFormattingTestData()
            {
                var dateFormats = new[]
                {
                    DateFormat.FromLocalizedDateFormat("YYYY-MM-DD"),
                    DateFormat.FromLocalizedDateFormat("DD.MM.YYYY"),
                    DateFormat.FromLocalizedDateFormat("DD-MM-YYYY"),
                    DateFormat.FromLocalizedDateFormat("MM/DD/YYYY"),
                    DateFormat.FromLocalizedDateFormat("DD/MM/YYYY"),
                    DateFormat.FromLocalizedDateFormat("MM-DD-YYYY")
                };

                var ranges = new[]
                {
                    (new DateTimeOffset(2017, 12, 15, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2017, 12, 15, 12, 34, 1, TimeSpan.Zero)),
                    (new DateTimeOffset(2017, 1, 1, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2017, 12, 30, 12, 34, 1, TimeSpan.Zero)),
                    (new DateTimeOffset(2017, 11, 13, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2018, 11, 13, 12, 34, 1, TimeSpan.Zero))
                };

                string expectedTitleString(DateTimeOffset start, DateTimeOffset end, DateFormat dateFormat)
                    => $"{start.ToString(dateFormat.Short)} - {end.ToString(dateFormat.Short)} ▾";

                foreach (var (start, end) in ranges)
                {
                    foreach (var dateFormat in dateFormats)
                    {
                        yield return new object[]
                        {
                            start,
                            end,
                            dateFormat,
                            expectedTitleString(start, end, dateFormat)
                        };
                    }
                }
            }
        }

        public sealed class TheSegmentsProperty : ReportsViewModelTest
        {
            private readonly int projectsNotSyncedCount = 0;

            [Fact]
            public async Task GroupsProjectSegmentsWithPercentageLessThanTenPercent()
            {
                ChartSegment[] segments =
                {
                    new ChartSegment("Project 1", "Client 1", 2, 2, 0, "#ffffff"),
                    new ChartSegment("Project 2", "Client 2", 7, 7, 0, "#ffffff"),
                    new ChartSegment("Project 3", "Client 3", 12, 12, 0, "#ffffff"),
                    new ChartSegment("Project 4", "Client 4", 23, 23, 0, "#ffffff"),
                    new ChartSegment("Project 5", "Client 5", 56, 56, 0, "#ffffff")
                };

                TimeService.CurrentDateTime.Returns(new DateTimeOffset(2018, 05, 15, 12, 00, 00, TimeSpan.Zero));
                ReportsProvider.GetProjectSummary(WorkspaceId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Return(new ProjectSummaryReport(segments, projectsNotSyncedCount)));

                await Initialize();

                ViewModel.Segments.Should().HaveCount(5);
                ViewModel.GroupedSegments.Should().HaveCount(4);
                ViewModel.GroupedSegments.Should().Contain(segment =>
                    segment.ProjectName == Resources.Other &&
                    segment.Percentage == segments[0].Percentage + segments[1].Percentage);
                ViewModel.GroupedSegments
                    .Where(project => project.ProjectName != Resources.Other)
                    .Select(segment => segment.Percentage)
                    .ForEach(percentage => percentage.Should().BeGreaterOrEqualTo(10));
            }

            [Fact]
            public async Task DoesNotGroupOtherProjectsWhenThereIsJustOneProjectWithLessThanTenPercent()
            {
                ChartSegment[] segments =
                {
                    new ChartSegment("Project 1", "Client 1", 7, 7, 0, "#ffffff"),
                    new ChartSegment("Project 2", "Client 2", 23, 23, 0, "#ffffff"),
                    new ChartSegment("Project 3", "Client 3", 70, 70, 0, "#ffffff")
                };

                TimeService.CurrentDateTime.Returns(new DateTimeOffset(2018, 05, 15, 12, 00, 00, TimeSpan.Zero));
                ReportsProvider.GetProjectSummary(WorkspaceId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Return(new ProjectSummaryReport(segments, projectsNotSyncedCount)));
                ViewModel.Prepare(WorkspaceId);

                await Initialize();

                ViewModel.GroupedSegments.Should().HaveCount(3);
                ViewModel.GroupedSegments.Should().NotContain(segment => segment.ProjectName == Resources.Other);
            }

            [Fact]
            public async Task DoesNotGroupOtherProjectsWhenAllProjectsHaveLessThanTenPercent()
            {
                ChartSegment[] segments =
                {
                    new ChartSegment("Project 1", "Client 1", 1, 1, 0, "#ffffff"),
                    new ChartSegment("Project 2", "Client 2", 3, 3, 0, "#ffffff"),
                    new ChartSegment("Project 3", "Client 3", 9, 9, 0, "#ffffff")
                };

                TimeService.CurrentDateTime.Returns(new DateTimeOffset(2018, 05, 15, 12, 00, 00, TimeSpan.Zero));
                ReportsProvider.GetProjectSummary(WorkspaceId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Return(new ProjectSummaryReport(segments, projectsNotSyncedCount)));
                ViewModel.Prepare(WorkspaceId);

                await Initialize();

                ViewModel.GroupedSegments.Should().HaveCount(3);
                ViewModel.GroupedSegments.Should().NotContain(segment => segment.ProjectName == Resources.Other);
            }
        }
    }
}
