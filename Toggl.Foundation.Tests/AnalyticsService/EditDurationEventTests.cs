using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Toggl.Foundation.Analytics;
using Xunit;

namespace Toggl.Foundation.Tests.AnalyticsService
{
    public sealed class EditDurationEventTests
    {

        [Fact, LogIfTooSlow]
        public void ResultingDictionaryHasAllParameters()
        {
            var editDurationEvent = new EditDurationEvent(false, EditDurationEvent.NavigationOrigin.Start);

            var dict = editDurationEvent.ToDictionary();

            dict.Count.Should().Be(12);
        }

        [Fact, LogIfTooSlow]
        public void SetsParametersUsingWith()
        {
            var editDurationEvent = new EditDurationEvent(false, EditDurationEvent.NavigationOrigin.Start);
            var changedEndTimeWithBarrel = editDurationEvent.ToDictionary()["changedEndTimeWithBarrel"];

            editDurationEvent = editDurationEvent.With(changedEndTimeWithBarrel: true);
            var dict = editDurationEvent.ToDictionary();

            changedEndTimeWithBarrel.Should().Be(false.ToString());
            dict["changedEndTimeWithBarrel"].Should().Be(true.ToString());
        }

        [Fact, LogIfTooSlow]
        public void SetsParametersUsingUpdateWith()
        {
            var editDurationEvent = new EditDurationEvent(false, EditDurationEvent.NavigationOrigin.Start);
            var changedEndTimeWithBarrel = editDurationEvent.ToDictionary()["changedEndTimeWithBarrel"];

            editDurationEvent = editDurationEvent.UpdateWith(EditTimeSource.BarrelStopTime);
            var dict = editDurationEvent.ToDictionary();

            changedEndTimeWithBarrel.Should().Be(false.ToString());
            dict["changedEndTimeWithBarrel"].Should().Be(true.ToString());
        }
    }
}
