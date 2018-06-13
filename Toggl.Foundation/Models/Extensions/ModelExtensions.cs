using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Models
{
    internal static class ModelExtensions
    {
        public static IThreadSafePreferences With(
            this IThreadSafePreferences original,
            New<TimeFormat> timeOfDayFormat = default(New<TimeFormat>),
            New<DateFormat> dateFormat = default(New<DateFormat>),
            New<DurationFormat> durationFormat = default(New<DurationFormat>),
            New<bool> collapseTimeEntries = default(New<bool>),
            New<SyncStatus> syncStatus = default(New<SyncStatus>),
            New<string> lastSyncErrorMessage = default(New<string>),
            New<bool> isDeleted = default(New<bool>)
        )
            => new Preferences(
                timeOfDayFormat.ValueOr(original.TimeOfDayFormat),
                dateFormat.ValueOr(original.DateFormat),
                durationFormat.ValueOr(original.DurationFormat),
                collapseTimeEntries.ValueOr(original.CollapseTimeEntries),
                syncStatus.ValueOr(original.SyncStatus),
                lastSyncErrorMessage.ValueOr(original.LastSyncErrorMessage),
                isDeleted.ValueOr(original.IsDeleted)
            );

        public static IThreadSafeProject With(
            this IThreadSafeProject original,
            New<DateTimeOffset> at = default(New<DateTimeOffset>)
        )
            => new Project(
                original.Id,
                original.Name,
                original.IsPrivate,
                original.Active,
                original.Color,
                original.Billable,
                original.Template,
                original.AutoEstimates,
                original.EstimatedHours,
                original.Rate,
                original.Currency,
                original.ActualHours,
                original.WorkspaceId,
                original.ClientId,
                original.SyncStatus,
                original.LastSyncErrorMessage,
                original.IsDeleted,
                at.ValueOr(original.At),
                original.ServerDeletedAt,
                original.Workspace,
                original.Client,
                original.Tasks
            );
    }
}
