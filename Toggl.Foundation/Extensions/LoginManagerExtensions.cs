using System;
using System.Reactive.Concurrency;
using Toggl.Foundation.DataSources;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Extensions
{
    public static class LoginManagerExtensions
    {
        private const int numberOfRetriesBeforeGivingUpOnFetchingAUserWithApiToken = 2;

        public static IObservable<ITogglDataSource> RetryWhenUserIsMissingApiToken(
            this IObservable<ITogglDataSource> observable, IScheduler scheduler)
        {
            return observable.ConditionalRetryWithBackoffStrategy(
                numberOfRetriesBeforeGivingUpOnFetchingAUserWithApiToken,
                backOffStrategyForDelayedRetry,
                exception => exception is UserIsMissingApiTokenException,
                scheduler);
        }

        private static TimeSpan backOffStrategyForDelayedRetry(int attempt)
        {
            var delayCap = TimeSpan.FromMinutes(1);
            var currentAttemptDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            return currentAttemptDelay >= delayCap ? delayCap : currentAttemptDelay;
        }
    }
}
