using System;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    interface ISyncSelector<T>
    {
        bool IsInSync(T model);
    }
}
