using System;

namespace Toggl.Multivac.Models
{
    public interface IWorkspace : IIdentifiable
    {
        string Name { get; }

        bool Admin { get; }

        DateTimeOffset? SuspendedAt { get; }

        DateTimeOffset? ServerDeletedAt { get; }

        double? DefaultHourlyRate { get; }

        string DefaultCurrency { get; }

        bool OnlyAdminsMayCreateProjects { get; }

        bool OnlyAdminsSeeBillableRates { get; }

        bool OnlyAdminsSeeTeamDashboard { get; }

        bool ProjectsBillableByDefault { get; }

        int Rounding { get; }

        int RoundingMinutes { get; }

        DateTimeOffset? At { get; }

        string LogoUrl { get; }
    }
}
