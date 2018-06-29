using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Firebase.RemoteConfig;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Daneel.Services
{
    public sealed class RemoteConfigService : IRemoteConfigService
    {
        public IObservable<RatingViewConfiguration> RatingViewConfiguration
            => Observable.Create<RatingViewConfiguration>( observer =>
            {
                var remoteConfig = RemoteConfig.SharedInstance;
                remoteConfig.ConfigSettings = new RemoteConfigSettings(true);
                remoteConfig.Fetch(0, (status, error) =>
                {
                    if (error != null)
                        observer.OnError(
                            new RemoteConfigFetchFailedException(error.ToString()));

                    remoteConfig.ActivateFetched();
                    var configuration = new RatingViewConfiguration(
                        remoteConfig["day_count"].NumberValue.Int32Value,
                        criterionStringToEnum(remoteConfig["criterion"].StringValue)
                    );
                    observer.OnNext(configuration);
                    observer.OnCompleted();
                });
                return Disposable.Empty;
            });

        private RatingViewCriterion criterionStringToEnum(string criterion)
        {
            switch (criterion)
            {
                case "stop":
                    return RatingViewCriterion.Stop;
                case "start":
                    return RatingViewCriterion.Start;
                case "continue":
                    return RatingViewCriterion.Continue;
                default:
                    return RatingViewCriterion.None;
            }
        }
    }
}
