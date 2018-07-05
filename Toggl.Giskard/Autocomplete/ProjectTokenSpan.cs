using Android.Graphics;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class ProjectTokenSpan : TokenSpan
    {
        public long ProjectId { get; }

        public string ProjectName { get; }

        public string ProjectColor { get; }

        public ProjectTokenSpan(long projectId, string projectName, string projectColor)
            : base(Color.White, Color.ParseColor(projectColor), false)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ProjectColor = projectColor;
        }
    }
}
