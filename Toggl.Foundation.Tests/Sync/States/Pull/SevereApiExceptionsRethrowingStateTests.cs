using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class SevereApiExceptionsRethrowingStateTests
    {
        private readonly SevereApiExceptionsRethrowingState state;

        public SevereApiExceptionsRethrowingStateTests()
        {
            state = new SevereApiExceptionsRethrowingState();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
        {
            var transition = state.Start(exception).SingleAsync().Wait();
            var reason = ((Transition<ApiException>)transition).Parameter;

            transition.Result.Should().Be(state.Retry);
            reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
        {
            var transition = state.Start(exception).SingleAsync().Wait();
            var reason = ((Transition<ApiException>)transition).Parameter;

            transition.Result.Should().Be(state.Retry);
            reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        [MemberData(nameof(ExtraExceptionsToRethrow))]
        public void ThrowsWhenExceptionsWhichShouldCauseRethrowAreCaught(ApiException exception)
        {
            Exception caughtException = null;

            try
            {
                state.Start(exception).Wait();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            caughtException.Should().NotBeNull();
            caughtException.Should().BeAssignableTo(exception.GetType());
        }

        public static IEnumerable<object[]> ExtraExceptionsToRethrow => new[]
        {
            new object[] { new DeserializationException<TestModel>(Substitute.For<IRequest>(), Substitute.For<IResponse>(), "{a:\"b\"}") }
        };
    }
}
