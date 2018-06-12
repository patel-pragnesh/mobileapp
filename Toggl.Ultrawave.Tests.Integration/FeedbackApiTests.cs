using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;

namespace Toggl.Ultrawave.Tests.Integration
{
    public sealed class FeedbackApiTests
    {
        public sealed class TheSendMethod : AuthenticatedPostEndpointBaseTests<Unit>
        {
            protected override IObservable<Unit> CallEndpointWith(ITogglApi togglApi)
                => togglApi.User.Get()
                    .SelectMany(user =>
                        togglApi.Feedback.Send(
                            email: user.Email,
                            message: "This is a test feedback msg.",
                            data: new Dictionary<string, string>
                            {
                                ["some key"] = "some value",
                                ["some other key"] = "some other value"
                            }));

            [Fact]
            public async Task DoesNotAcceptFeedbackFromADifferentUserFromTheLoggedIn()
            {
                var (togglApi, user) = await SetupTestUser();
                var email = Email.From($"{Guid.NewGuid()}@toggl.space");

                Action sendingFeedback = async () => await togglApi.Feedback.Send(email, "ABC.", new Dictionary<string, string>());

                sendingFeedback.Should().Throw<BadRequestException>();
            }
        }
    }
}
