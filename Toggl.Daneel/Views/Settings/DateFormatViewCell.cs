using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public sealed partial class DateFormatViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(DateFormatViewCell));
        public static readonly UINib Nib;

        static DateFormatViewCell()
        {
            Nib = UINib.FromName(nameof(DateFormatViewCell), NSBundle.MainBundle);
        }

        protected DateFormatViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<DateFormatViewCell, SelectableDateFormatViewModel>();

                bindingSet.Bind(DateFormatLabel).To(vm => vm.DateFormat.Localized);

                bindingSet.Bind(SelectedImageView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.Selected);

                bindingSet.Apply();
            });
        }
    }
}
