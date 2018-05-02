using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Foundation.Sync.States.RetryLoop;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class CheckServerStatusStateTests
    {
        private readonly ITogglApi api;
        private readonly TestScheduler scheduler;
        private readonly IRetryDelayService apiDelay;
        private readonly IRetryDelayService statusDelay;
        private readonly CheckServerStatusState state;
        private readonly ISubject<Unit> delayCancellation;
        private readonly IState serverIsAvailableState;

        public CheckServerStatusStateTests()
        {
            api = Substitute.For<ITogglApi>();
            scheduler = new TestScheduler();
            apiDelay = Substitute.For<IRetryDelayService>();
            statusDelay = Substitute.For<IRetryDelayService>();
            serverIsAvailableState = Substitute.For<IState>();
            delayCancellation = new Subject<Unit>();
            state = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation.AsObservable(), serverIsAvailableState);
        }

        [Fact, LogIfTooSlow]
        public void ReturnsTheServerIsAvailableStateWhenTheStatusEndpointReturnsOK()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(1));

            IResult result = null;
            state.Run().Subscribe(r => result = r);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            result.Should().BeOfType<Proceed>();
        }

        [Fact, LogIfTooSlow]
        public void DoesNotResetTheStatusDelayServiceWhenTheStatusEndpointReturnsOKBeforeTheApiSlowDelay()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(1));

            state.Run().Subscribe(_ => { });
            scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks - 1);

            statusDelay.DidNotReceive().Reset();
        }

        [Fact, LogIfTooSlow]
        public void ResetsTheStatusDelayServiceWhenTheStatusEndpointReturnsOKAfterTheApiSlowDelay()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(1));

            state.Run().Subscribe(_ => { });
            scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            statusDelay.Received().Reset();
        }

        [Fact, LogIfTooSlow]
        public void DelaysTheTransitionByAtLeastTheSlowApiDelayTimeWhenTheStatusEndpointReturnsOK()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(1));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));
            statusDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(1));
            statusDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(1));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks - 1);
            subscription.Dispose();

            hasCompleted.Should().BeFalse();
        }

        [Fact, LogIfTooSlow]
        public void DelaysTheTransitionByAtMostTheSlowApiDelayTimeWhenTheStatusEndpointReturnsOK()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(100));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));
            statusDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(100));
            statusDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(100));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            subscription.Dispose();

            hasCompleted.Should().BeTrue();
        }

        [Fact, LogIfTooSlow]
        public void DelaysTheTransitionAtMostByTheNextSlowDelayTimeFromTheRetryDelayServiceWhenInternalServerErrorOccurs()
        {
            var observable = Observable.Throw<Unit>(new InternalServerErrorException(request, response));
            api.Status.IsAvailable().Returns(observable);
            apiDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(100));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            subscription.Dispose();

            hasCompleted.Should().BeTrue();
        }

        [Fact, LogIfTooSlow]
        public void DelaysTheTransitionAtLeastByTheNextSlowDelayTimeFromTheRetryDelayServiceWhenInternalServerErrorOccurs()
        {
            var observable = Observable.Throw<Unit>(new InternalServerErrorException(request, response));
            api.Status.IsAvailable().Returns(observable);
            statusDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(1));
            statusDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks - 1);
            subscription.Dispose();

            hasCompleted.Should().BeFalse();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ServerExceptionsOtherThanInternalServerErrorException))]
        public void DelaysTheTransitionAtMostByTheNextFastDelayTimeFromTheRetryDelayServiceWhenAServerErrorOtherThanInternalServerErrorOccurs(ServerErrorException exception)
        {
            api.Status.IsAvailable().Returns(Observable.Throw<Unit>(exception));
            statusDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(10));
            statusDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(100));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks + 1);
            subscription.Dispose();

            hasCompleted.Should().BeTrue();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ServerExceptionsOtherThanInternalServerErrorException))]
        public void DelaysTheTransitionAtLeastByTheNextFastDelayTimeFromTheRetryDelayServiceWhenAServerErrorOtherThanInternalServerErrorOccurs(ServerErrorException exception)
        {
            api.Status.IsAvailable().Returns(Observable.Throw<Unit>(exception));
            statusDelay.NextFastDelay().Returns(TimeSpan.FromSeconds(10));
            statusDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(1));
            var hasCompleted = false;

            var subscription = state.Run().Subscribe(_ => hasCompleted = true);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks - 1);
            subscription.Dispose();

            hasCompleted.Should().BeFalse();
        }

        [Fact, LogIfTooSlow]
        public void CompletesEvenThoughTheDelayIsNotOverButTheCancellationObservableIsNotifiedOfNewValue()
        {
            api.Status.IsAvailable().Returns(Observable.Return(Unit.Default));
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));

            IResult result = null;
            state.Run().Subscribe(r => result = r);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
            delayCancellation.OnNext(Unit.Default);

            result.Should().BeOfType<Proceed>();
            ((Proceed)result).NextState.Should().Be(serverIsAvailableState);
        }

        [Fact, LogIfTooSlow]
        public void CompletesEvenThoughTheRetryDelayIsNotOverButTheCancellationObservableIsNotifiedOfNewValue()
        {
            var observable = Observable.Throw<Unit>(new InternalServerErrorException(request, response));
            api.Status.IsAvailable().Returns(observable);
            apiDelay.NextSlowDelay().Returns(TimeSpan.FromSeconds(10));

            IResult result = null;
            state.Run().Subscribe(r => result = r);
            scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
            delayCancellation.OnNext(Unit.Default);

            result.Should().BeOfType<Proceed>();
            ((Proceed)result).NextState.Should().Be(state);
        }

        public static IEnumerable<object[]> ServerExceptionsOtherThanInternalServerErrorException()
            => new[]
            {
                new object[] { new BadGatewayException(request, response) },
                new object[] { new GatewayTimeoutException(request, response) },
                new object[] { new HttpVersionNotSupportedException(request, response) },
                new object[] { new ServiceUnavailableException(request, response) }
            };

        private static IRequest request => Substitute.For<IRequest>();

        private static IResponse response => Substitute.For<IResponse>();
    }
}
