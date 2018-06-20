using System;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IWorkspacesApi
        : IPullingApiClient<IWorkspace>,
          ICreatingApiClient<IWorkspace>
    {
        IObservable<IWorkspace> GetById(long id);
    }
}
