using Toggl.Foundation.Models;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UnsyncablePreferencesState : BaseUnsyncableEntityState<IDatabasePreferences>
    {
        public UnsyncablePreferencesState(IRepository<IDatabasePreferences> repository) : base(repository)
        {
        }

        protected override bool HasChanged(IDatabasePreferences original, IDatabasePreferences current)
            => original.IsEqualTo(current) == false;

        protected override IDatabasePreferences CreateUnsyncableFrom(IDatabasePreferences entity, string reason)
            => Preferences.Unsyncable(entity, reason);

        protected override IDatabasePreferences CopyFrom(IDatabasePreferences entity)
            => Preferences.From(entity);
    }
}
