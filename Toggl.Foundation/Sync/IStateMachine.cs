using System;
using System.Threading.Tasks;

namespace Toggl.Foundation.Sync
{
    public interface IStateMachine
    {
        Task Run(ISyncStateQueue queue, IObservable<bool> abort);
    }
}
