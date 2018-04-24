using System;
using Toggl.Foundation.Sync.ConflictResolution.Selectors;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    internal static class Resolver
    {
        public static IConflictResolver<IDatabaseClient> ForClients()
            => new PreferNewer<IDatabaseClient>();

        public static IConflictResolver<IDatabaseProject> ForProjects()
            => new PreferNewer<IDatabaseProject>();

        internal static IConflictResolver<IDatabaseUser> ForUser()
            => new PreferNewer<IDatabaseUser>();

        public static IConflictResolver<IDatabaseWorkspace> ForWorkspaces()
            => new PreferNewer<IDatabaseWorkspace>();

        internal static IConflictResolver<IDatabasePreferences> ForPreferences()
            => new OverwriteUnlessNeedsSync<IDatabasePreferences>();

        public static IConflictResolver<IDatabaseWorkspaceFeatureCollection> ForWorkspaceFeatures()
            => new AlwaysOverwrite<IDatabaseWorkspaceFeatureCollection>();

        public static IConflictResolver<IDatabaseTask> ForTasks()
            => new PreferNewer<IDatabaseTask>();

        public static IConflictResolver<IDatabaseTag> ForTags()
            => new PreferNewer<IDatabaseTag>();

        public static IConflictResolver<IDatabaseTimeEntry> ForTimeEntries()
            => new PreferNewer<IDatabaseTimeEntry>(TimeSpan.FromSeconds(5));
    }
}
