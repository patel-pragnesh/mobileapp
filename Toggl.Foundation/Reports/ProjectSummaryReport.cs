using System.Linq;

namespace Toggl.Foundation.Reports
{
    public sealed class ProjectSummaryReport
    {
        private const float textDrawingThreshold = 0.1f;
        
        public float TotalSeconds { get; }

        public float BillablePercentage { get; }

        public ChartSegment[] Segments { get; }

        public ProjectSummaryReport(ChartSegment[] segments)
        {
            var totalSeconds = segments.Select(x => x.TrackedTime.TotalSeconds).Sum();
            var billableSeconds = segments.Select(x => x.BillableSeconds).Sum();

            Segments = segments;
            TotalSeconds = (float)totalSeconds;
            BillablePercentage = (float)(totalSeconds > 0 ? (100.0f / totalSeconds) * billableSeconds : 0);
        }

        public static bool ShouldDraw(float percent)
            => percent > textDrawingThreshold;
    }
}
