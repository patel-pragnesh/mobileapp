using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public sealed partial class LicensesViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(LicensesViewCell));
        public static readonly UINib Nib;

        public string Text
        {
            get => LicenseLabel.Text;
            set => LicenseLabel.Text = value;
        }

        static LicensesViewCell()
        {
            Nib = UINib.FromName(nameof(LicensesViewCell), NSBundle.MainBundle);
        }

        protected LicensesViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            GrayBackground.Layer.BorderWidth = 1;
            GrayBackground.Layer.BorderColor = Color.Licenses.Border.ToNativeColor().CGColor;
        }
    }
}
