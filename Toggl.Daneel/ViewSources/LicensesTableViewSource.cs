using System;
using Foundation;
using MvvmCross.Binding.Extensions;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views.Settings;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class LicensesTableViewSource : MvxTableViewSource
    {
        private const string cellIdentifier = nameof(LicensesViewCell);
        private const string headerIdentifier = nameof(LicensesHeaderViewCell);

        public LicensesTableViewSource(UITableView tableView) : base(tableView)
        {
            tableView.RegisterNibForCellReuse(LicensesViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(LicensesHeaderViewCell.Nib, headerIdentifier);
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = (License)GetItemAt(indexPath);
            var cell = (LicensesViewCell)GetOrCreateCellFor(tableView, indexPath, item);

            cell.Text = item.Text;

            return cell;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var item = ItemsSource.ElementAt((int)section);

            var header = tableView.DequeueReusableHeaderFooterView(headerIdentifier);
            if (header is IMvxBindable bindable)
                bindable.DataContext = item;

            return header;
        }

        protected override object GetItemAt(NSIndexPath indexPath)
            => ItemsSource.ElementAt(indexPath.Section);

        public override nint NumberOfSections(UITableView tableView) => ItemsSource.Count();

        public override nint RowsInSection(UITableView tableview, nint section)
            => 1;
    }
}
