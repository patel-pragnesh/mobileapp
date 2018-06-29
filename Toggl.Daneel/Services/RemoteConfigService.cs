using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Firebase.RemoteConfig;
using MvvmCross.Platform;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.MvvmCross.Services;
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
                    showAlert(configuration);
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

        private void showAlert(RatingViewConfiguration configuration)
        {
            var dialogService = Mvx.Resolve<IDialogService>();
            dialogService
                .Alert(
                    "Got remote config!",
                    $"Day count: {configuration.DayCount}, criterion: {configuration.Criterion.ToString()}",
                    "Fo shizzle, ma nizzle")
                .Subscribe((Unit _) => { });
        }
    }
}
