using MvvmCross.iOS.Views;
using Toggl.Daneel.Cells.Settings;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public sealed class LicensesViewController : MvxTableViewController<LicensesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = Resources.Licenses;

            TableView.Source = new LicensesTableViewSource(ViewModel.Licenses);
            TableView.EstimatedRowHeight = 396;
            TableView.SectionHeaderHeight = 44;
            TableView.RowHeight = UITableView.AutomaticDimension;

            TableView.RegisterNibForCellReuse(LicensesViewCell.Nib, LicensesTableViewSource.CellIdentifier);
            TableView.RegisterNibForHeaderFooterViewReuse(LicensesHeaderViewCell.Nib, LicensesTableViewSource.HeaderIdentifier);
        }
    }
}
