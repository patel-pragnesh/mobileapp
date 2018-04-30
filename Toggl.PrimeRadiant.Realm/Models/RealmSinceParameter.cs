using System;
using Realms;
using Toggl.PrimeRadiant.Models;

namespace Toggl.PrimeRadiant.Realm.Models
{
    internal sealed class RealmSinceParameter : RealmObject, ISinceParameter
    {
        [PrimaryKey]
        public string Key { get; set; }

        public DateTimeOffset? Since { get; set; }
    }
}
