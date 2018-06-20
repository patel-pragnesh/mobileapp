﻿using System;
using FluentAssertions;
using FsCheck.Xunit;
using Toggl.Foundation.Sync;
using Xunit;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class RetryDelayServiceTests
    {
        [Fact, LogIfTooSlow]
        public void TheDefaultFastDelayIsTenSeconds()
        {
            var delay = new RetryDelayService(new Random());

            var totalDelay = delay.NextFastDelay();

            totalDelay.TotalSeconds.Should().Be(10);
        }

        [Fact, LogIfTooSlow]
        public void TheDefaultSlowDelayIsSixtySeconds()
        {
            var delay = new RetryDelayService(new Random());

            var totalDelay = delay.NextSlowDelay();

            totalDelay.TotalSeconds.Should().Be(60);
        }

        [Property]
        public void ThrowsWhenTheSlowDelayExceedsTheLimit(byte limitsInSeconds)
        {
            if (limitsInSeconds == 0)
                return;

            var limit = TimeSpan.FromSeconds(limitsInSeconds);
            var delay = new RetryDelayService(new Random(), limit);
            TimeSpan maxDelay = TimeSpan.Zero;

            Action waitingTooLong = () =>
            {
                while (true)
                {
                    var nextDelay = delay.NextSlowDelay();
                    maxDelay = maxDelay > nextDelay ? maxDelay : nextDelay;
                }
            };

            waitingTooLong.Should().Throw<TimeoutException>();
            maxDelay.Should().BeLessOrEqualTo(limit);
        }

        [Property]
        public void ThrowsWhenTheFastDelayExceedsTheLimit(byte limitsInSeconds)
        {
            if (limitsInSeconds == 0)
                return;

            var limit = TimeSpan.FromSeconds(limitsInSeconds);
            var delay = new RetryDelayService(new Random(), limit);
            TimeSpan maxDelay = TimeSpan.Zero;

            Action waitingTooLong = () =>
            {
                while (true)
                {
                    var nextDelay = delay.NextFastDelay();
                    maxDelay = maxDelay > nextDelay ? maxDelay : nextDelay;
                }
            };

            waitingTooLong.Should().Throw<TimeoutException>();
            maxDelay.Should().BeLessOrEqualTo(limit);
        }

        [Property]
        public void DoesNotThrowWhenUsingSlowDelayAndWhenThereIsNotLimit(byte limitsInSeconds)
        {
            var limit = TimeSpan.FromSeconds(limitsInSeconds);
            var delay = new RetryDelayService(new Random(), null);
            TimeSpan maxDelay = TimeSpan.Zero;

            Action waitingTooLong = () =>
            {
                do
                {
                    var nextDelay = delay.NextSlowDelay();
                    maxDelay = maxDelay > nextDelay ? maxDelay : nextDelay;
                } while (maxDelay <= limit);
            };

            waitingTooLong.Should().NotThrow();
            maxDelay.Should().BeGreaterThan(limit);
        }

        [Property]
        public void DoesNotThrowWhenUsingFastDelayAndWhenThereIsNotLimit(byte limitsInSeconds)
        {
            var limit = TimeSpan.FromSeconds(limitsInSeconds);
            var delay = new RetryDelayService(new Random(), null);
            TimeSpan maxDelay = TimeSpan.Zero;

            Action waitingTooLong = () =>
            {
                do
                {
                    var nextDelay = delay.NextFastDelay();
                    maxDelay = maxDelay > nextDelay ? maxDelay : nextDelay;
                } while (maxDelay <= limit);
            };

            waitingTooLong.Should().NotThrow();
            maxDelay.Should().BeGreaterThan(limit);
        }

        [Property]
        public void TheResetMethodClearsAnyHistoryAndTheServiceReturnsTheDefaultValueForSlowDelay(int seed, bool[] slowOrFast)
        {
            var delay = new RetryDelayService(new Random(seed));
            runDelays(delay, slowOrFast);

            delay.Reset();
            var totalDelay = delay.NextSlowDelay();

            totalDelay.TotalSeconds.Should().Be(60);
        }

        [Property]
        public void TheResetMethodClearsAnyHistoryAndTheServiceReturnsTheDefaultValueForFastDelay(int seed, bool[] slowOrFast)
        {
            var delay = new RetryDelayService(new Random(seed));
            runDelays(delay, slowOrFast);

            delay.Reset();
            var totalDelay = delay.NextFastDelay();

            totalDelay.TotalSeconds.Should().Be(10);
        }

        [Property]
        public void AnySequenceOfDelaysStaysWithinTheExpectedBounds(int seed, bool[] slowOrFast)
        {
            double minDelay = 0;
            double maxDelay = 0;
            TimeSpan totalDelay = TimeSpan.Zero;
            var delay = new RetryDelayService(new Random(seed));

            foreach (var nextIsSlow in slowOrFast)
            {
                if (nextIsSlow)
                {
                    if (minDelay == 0)
                    {
                        minDelay = 60;
                        maxDelay = 60;
                    }
                    else
                    {
                        minDelay *= 1.5;
                        maxDelay *= 2;
                    }

                    totalDelay = delay.NextSlowDelay();
                }
                else
                {
                    if (minDelay == 0)
                    {
                        minDelay = 10;
                        maxDelay = 10;
                    }
                    else
                    {
                        minDelay *= 1;
                        maxDelay *= 1.5;
                    }

                    totalDelay = delay.NextFastDelay();
                }
            }

            var minTotalDelay = Math.Min(TimeSpan.MaxValue.TotalSeconds, minDelay);
            var maxTotalDelay = Math.Min(TimeSpan.MaxValue.TotalSeconds, maxDelay);
            totalDelay.TotalSeconds.Should()
                .BeGreaterOrEqualTo(minTotalDelay).And
                .BeLessOrEqualTo(maxTotalDelay);
        }

        private void runDelays(IRetryDelayService delay, bool[] slowOrFast)
        {
            foreach (var nextIsSlow in slowOrFast)
            {
                if (nextIsSlow)
                    delay.NextSlowDelay();
                else
                    delay.NextFastDelay();
            }
        }
    }
}
