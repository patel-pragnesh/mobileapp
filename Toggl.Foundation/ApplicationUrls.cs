namespace Toggl.Foundation
{
    public static class ApplicationUrls
    {
        public const string Reports = "toggl://reports";

        public const string StartTimeEntry = "toggl://start";

        public static class Main
        {
            public static class Action
            {
                public const string Stop = "stop";
                public const string Continue = "continue";
            }

            public const string Regex = @"toggl://main\?action=(?<action>.+)";

            public static readonly string StopTimeEntry = $"toggl://main?action={Action.Stop}";

            public static readonly string ContinueLastEntry = $"toggl://main?action={Action.Continue}";
        }
    }
}
