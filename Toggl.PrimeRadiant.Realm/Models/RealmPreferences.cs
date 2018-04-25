using Realms;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using System;

namespace Toggl.PrimeRadiant.Realm
{
    internal partial class RealmPreferences : RealmObject, IDatabasePreferences
    {
        [Ignored]
        public TimeFormat TimeOfDayFormat
        {
            get => TimeFormat.FromLocalizedTimeFormat(TimeOfDayFormatString);
            set => TimeOfDayFormatString = value.Localized;
        }

        public string TimeOfDayFormatString { get; set; }

        [Ignored]
        public DateFormat DateFormat
        {
            get => DateFormat.FromLocalizedDateFormat(DateFormatString);
            set => DateFormatString = value.Localized;
        }

        public string DateFormatString { get; set; }

        [Ignored]
        public DurationFormat DurationFormat
        {
            get => (DurationFormat)DurationFormatInt;
            set => DurationFormatInt = (int)value;
        }

        public int DurationFormatInt { get; set; }

        public bool CollapseTimeEntries { get; set; }
    }
}
