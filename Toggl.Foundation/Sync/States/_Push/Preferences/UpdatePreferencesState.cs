using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UpdatePreferencesState : BaseUpdateEntityState<IDatabasePreferences>
    {
        public UpdatePreferencesState(ITogglApi api, IRepository<IDatabasePreferences> repository) : base(api, repository)
        {
        }

        protected override bool HasChanged(IDatabasePreferences original, IDatabasePreferences current)
            => original.IsEqualTo(current) == false;

        protected override IObservable<IDatabasePreferences> Update(IDatabasePreferences entity)
            => Api.Preferences.Update(entity).Select(Preferences.Clean);

        protected override IDatabasePreferences CopyFrom(IDatabasePreferences entity)
            => Preferences.From(entity);
    }
}
