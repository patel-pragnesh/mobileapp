using System;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Analytics
{
    internal delegate IAnalyticsEvent<string> SyncAnalyticsExtensionsSearchStrategy(Type entityType, IAnalyticsService analyticsService);

    public static class SyncAnalyticsExtensions
    {
        public static IAnalyticsEvent<string> ToSyncErrorAnalyticsEvent(this Type entityType, IAnalyticsService analyticsService)
        {
            var searchStrategy = SearchStrategy ?? DefaultSyncAnalyticsExtensionsSearchStrategy;
            return searchStrategy(entityType, analyticsService);
        }

        internal static SyncAnalyticsExtensionsSearchStrategy SearchStrategy { get; set; }
            = DefaultSyncAnalyticsExtensionsSearchStrategy;

        internal static IAnalyticsEvent<string> DefaultSyncAnalyticsExtensionsSearchStrategy(Type entityType, IAnalyticsService analyticsService)
        {
            if (typeof(IThreadSafeWorkspace).IsAssignableFrom(entityType))
                return analyticsService.WorkspaceSyncError;
           
            if (typeof(IThreadSafeUser).IsAssignableFrom(entityType))
                return analyticsService.UserSyncError;

            if (typeof(IThreadSafeWorkspaceFeature).IsAssignableFrom(entityType))
                return analyticsService.WorkspaceFeaturesSyncError;
           
            if (typeof(IThreadSafePreferences).IsAssignableFrom(entityType))
                return analyticsService.PreferencesSyncError;
           
            if (typeof(IThreadSafeTag).IsAssignableFrom(entityType))
                return analyticsService.TagsSyncError;

            if (typeof(IThreadSafeClient).IsAssignableFrom(entityType))
                return analyticsService.ClientsSyncError;

            if (typeof(IThreadSafeProject).IsAssignableFrom(entityType))
                return analyticsService.ProjectsSyncError;

            if (typeof(IThreadSafeTask).IsAssignableFrom(entityType))
                return analyticsService.TasksSyncError;

            if (typeof(IThreadSafeTimeEntry).IsAssignableFrom(entityType))
                return analyticsService.TimeEntrySyncError;
               
            throw new ArgumentException($"The entity '{entityType.Name}' has no corresponding analytics events and should not be used.");
        }
    }
}
