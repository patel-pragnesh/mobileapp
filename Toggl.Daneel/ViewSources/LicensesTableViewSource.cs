using System;
using System.Collections.Immutable;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Cells.Settings;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class LicensesTableViewSource : UITableViewSource
    {
        public const string CellIdentifier = nameof(LicensesViewCell);
        public const string HeaderIdentifier = nameof(LicensesHeaderViewCell);

        private readonly IImmutableList<License> items;

        public LicensesTableViewSource(IImmutableList<License> items)
        {
            this.items = items;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath) as BaseTableViewCell<License>;
            cell.Item = items[indexPath.Row];
            return cell;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var item = items[(int)section];

            var header = tableView.DequeueReusableHeaderFooterView(HeaderIdentifier) as LicensesHeaderViewCell;
            header.Item = item;

            return header;
        }

        public override nint NumberOfSections(UITableView tableView) => items.Count;

        public override nint RowsInSection(UITableView tableview, nint section) => 1;
    }
}
