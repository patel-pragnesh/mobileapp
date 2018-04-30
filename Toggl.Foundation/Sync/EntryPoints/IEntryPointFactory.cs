using Toggl.Foundation.Sync.States;

namespace Toggl.Foundation.Sync.EntryPoints
{
    public interface IEntryPointFactory
    {
        IState Create();
    }
}
