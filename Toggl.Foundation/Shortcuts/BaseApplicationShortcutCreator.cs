using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Shortcuts
{
    public abstract class BaseApplicationShortcutCreator : IApplicationShortcutCreator
    {
        private bool noTimeEntries;

        private readonly ApplicationShortcut reportsShortcut = new ApplicationShortcut(
            ApplicationUrls.Reports,
            Resources.Reports,
            ShortcutType.Reports
        );

        private readonly ApplicationShortcut startTimeEntryShortcut = new ApplicationShortcut(
            ApplicationUrls.StartTimeEntry,
            Resources.StartTimeEntry,
            ShortcutType.StartTimeEntry
        );

        private readonly ApplicationShortcut continueLastEntryShortcut = new ApplicationShortcut(
            ApplicationUrls.Main.ContinueLastEntry,
            Resources.ContinueLastEntry,
            ShortcutType.ContinueLastTimeEntry
        );

        public void OnLogin(ITogglDataSource dataSource)
        {
            if (dataSource == null) return;

            SetShortcuts(new[] { reportsShortcut, startTimeEntryShortcut });

            dataSource
                .TimeEntries
                .IsEmpty
                .Subscribe(isEmpty => noTimeEntries = isEmpty);

            dataSource
                .TimeEntries
                .CurrentlyRunningTimeEntry
                .Subscribe(onCurrentTimeEntryChanged);
        }

        public void OnLogout()
            => ClearAllShortCuts();

        private void onCurrentTimeEntryChanged(IThreadSafeTimeEntry timeEntry)
        {
            var shortcuts = new List<ApplicationShortcut> { startTimeEntryShortcut };

            if (timeEntry == null && !noTimeEntries)
                shortcuts.Add(continueLastEntryShortcut);

            shortcuts.Add(reportsShortcut);

            SetShortcuts(shortcuts);
        }

        protected abstract void ClearAllShortCuts();
        protected abstract void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts);
    }
}
