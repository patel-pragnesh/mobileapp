using NUnit.Framework;

namespace Toggl.Tests.UI
{
    public class ReplTests
    {
        [Test, IgnoreOnIos, IgnoreOnAndroid]
        public void Repl()
        {
            var app = Configuration.GetApp();
            app.Repl();
        }
    }
}
