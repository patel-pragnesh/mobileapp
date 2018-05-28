using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public sealed partial class LicensesHeaderViewCell : MvxTableViewHeaderFooterView
    {
        public static readonly NSString Key = new NSString(nameof(LicensesHeaderViewCell));
        public static readonly UINib Nib;

        static LicensesHeaderViewCell()
        {
            Nib = UINib.FromName(nameof(LicensesHeaderViewCell), NSBundle.MainBundle);
        }

        protected LicensesHeaderViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<LicensesHeaderViewCell, License>();

                bindingSet.Bind(HeaderLabel).To(vm => vm.Subject);

                bindingSet.Apply();
            });
        }
    }
}
