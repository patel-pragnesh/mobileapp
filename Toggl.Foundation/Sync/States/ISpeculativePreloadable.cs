using System;
using System.Collections.Generic;

namespace Toggl.Foundation.Sync.States
{
    public interface ISpeculativePreloadable
    {
        IEnumerable<ISpeculativePreloadable> Preload();
    }
}
