using System;
using System.Reactive.Linq;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Giskard.Services
{
    public class RemoteConfigService : IRemoteConfigService
    {
        public IObservable<RatingViewConfiguration> RatingViewConfiguration { get; }
            = Observable.Return(new RatingViewConfiguration(int.MaxValue, RatingViewCriterion.None));
    }
}
