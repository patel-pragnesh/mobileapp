using System.Collections.Generic;

namespace Toggl.Multivac.Models
{
    public interface IWorkspaceFeatureCollection : IIdentifiable, ISyncable
    {
        long WorkspaceId { get; }

        IEnumerable<IWorkspaceFeature> Features { get; }

        bool IsEnabled(WorkspaceFeatureId feature);
    }
}
