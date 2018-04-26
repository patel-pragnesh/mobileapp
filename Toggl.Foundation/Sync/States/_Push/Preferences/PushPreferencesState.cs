using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushPreferencesState : BasePushState<IDatabasePreferences>
    {
        public PushPreferencesState(IRepository<IDatabasePreferences> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabasePreferences entity)
            => DateTimeOffset.Now;

        protected override IDatabasePreferences CopyFrom(IDatabasePreferences entity)
            => Preferences.From(entity);
    }
}
