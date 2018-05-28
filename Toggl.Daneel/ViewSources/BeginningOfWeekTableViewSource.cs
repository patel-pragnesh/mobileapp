using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views.Settings;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class BeginningOfWeekTableViewSource : MvxTableViewSource
    {
        private const string cellIdentifier = nameof(DayOfWeekViewCell);

        public BeginningOfWeekTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(DayOfWeekViewCell.Nib, cellIdentifier);
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
    }
}
