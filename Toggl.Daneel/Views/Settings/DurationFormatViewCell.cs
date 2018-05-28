using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public sealed partial class DurationFormatViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(DurationFormatViewCell));
        public static readonly UINib Nib;

        static DurationFormatViewCell()
        {
            Nib = UINib.FromName(nameof(DurationFormatViewCell), NSBundle.MainBundle);
        }

        protected DurationFormatViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<DurationFormatViewCell, SelectableDurationFormatViewModel>();

                bindingSet.Bind(DurationFormatLabel)
                          .To(vm => vm.DurationFormat)
                          .WithConversion(new DurationFormatToStringValueConverter());

                bindingSet.Bind(SelectedImageView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.Selected);

                bindingSet.Apply();
            });
        }
    }
}
