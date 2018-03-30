using Toggl.Foundation.DataSources;

namespace Toggl.Foundation.Shortcuts
{
    public interface IApplicationShortcutCreator
    {
        void OnLogin(ITogglDataSource dataSource);
        void OnLogout();
    }
}
