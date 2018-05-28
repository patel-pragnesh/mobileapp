using System;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views.Settings;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class DurationFormatsTableViewSource : MvxTableViewSource
    {
        private const string cellIdentifier = nameof(DateFormatViewCell);

        public DurationFormatsTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(DurationFormatViewCell.Nib, cellIdentifier);
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;
            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 48;
    }
}
