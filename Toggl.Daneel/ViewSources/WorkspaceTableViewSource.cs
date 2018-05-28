using System;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class WorkspaceTableViewSource : MvxTableViewSource
    {
        private const int rowHeight = 64;
        private const int headerHeight = 45;
        private const string cellIdentifier = nameof(WorkspaceViewCell);

        public WorkspaceTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(WorkspaceViewCell.Nib, cellIdentifier);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (item != null && cell is IMvxBindable bindable)
                bindable.DataContext = item;

            return cell;
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 48;
    }
}
