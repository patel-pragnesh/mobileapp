using System;
using System.Collections.Generic;
using Toggl.Foundation.Sync.States;

namespace Toggl.Foundation.Extensions
{
    public static class SpeculativePreloadableExtensions
    {
        public static void PreloadRecursively(this ISpeculativePreloadable preloadable)
        {
            IEnumerable<ISpeculativePreloadable> possibleFollowUpPreloadable;
            try
            {
                possibleFollowUpPreloadable = preloadable.Preload();
            }
            catch
            {
                // todo: what to do now??? silent error?
                return;
            }

            foreach (var followUp in possibleFollowUpPreloadable)
                followUp.PreloadRecursively();
        }
    }
}
