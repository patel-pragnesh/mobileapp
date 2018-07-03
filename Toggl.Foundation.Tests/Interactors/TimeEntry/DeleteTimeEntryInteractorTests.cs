﻿using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant.DTOs;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors
{
    public sealed class DeleteTimeEntryInteractorTests : BaseInteractorTests
    {
        private readonly MockTimeEntry timeEntry;

        public DeleteTimeEntryInteractorTests()
        {
            timeEntry = new MockTimeEntry
            {
                Id = 12
            };

            DataSource.TimeEntries.GetById(timeEntry.Id)
                .Returns(Observable.Return(timeEntry));
            DataSource.TimeEntries.Update(Arg.Any<TimeEntryDto>())
                .Returns(callInfo => Observable.Return(callInfo.Arg<IThreadSafeTimeEntry>()));
        }

        [Fact]
        public async Task UpdatesTheEntityWithASoftDeletedEntity()
        {
            await InteractorFactory.DeleteTimeEntry(timeEntry.Id).Execute();

            await DataSource.TimeEntries.Received().SoftDelete(
                Arg.Is<IThreadSafeTimeEntry>(timeEntry));
        }
    }
}
