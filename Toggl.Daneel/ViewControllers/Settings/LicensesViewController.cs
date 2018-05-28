using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Views;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public sealed partial class LicensesViewController : MvxViewController<LicensesViewModel>
    {
        public LicensesViewController() : base(nameof(LicensesViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TopConstraint.AdaptForIos10(NavigationController.NavigationBar);
            Title = Resources.Licenses;

            LicensesTableView.RowHeight = UITableView.AutomaticDimension;
            LicensesTableView.EstimatedRowHeight = 396;
            LicensesTableView.SectionHeaderHeight = 44;

            var source = new LicensesTableViewSource(LicensesTableView);
            LicensesTableView.Source = source;

            var bindingSet = this.CreateBindingSet<LicensesViewController, LicensesViewModel>();

            bindingSet.Bind(source).To(vm => vm.Licenses);

            bindingSet.Apply();
        }
    }
}
