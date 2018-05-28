using System.Collections.Generic;
using MvvmCross.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Collections
{
    [Preserve(AllMembers = true)]
    public class WorkspaceGroupedCollection<T> : MvxObservableCollection<T>
    {
        public long WorkspaceId { get; }
        public string WorkspaceName { get; }

        public WorkspaceGroupedCollection()
        {
        }

        public WorkspaceGroupedCollection(string workspaceName, long workspaceId, IEnumerable<T> items)
        {
            Ensure.Argument.IsNotNull(items, nameof(items));
            Ensure.Argument.IsNotNull(workspaceName, nameof(workspaceName));

            WorkspaceName = workspaceName;
            WorkspaceId = workspaceId;
            AddRange(items);
        }
    }
}
