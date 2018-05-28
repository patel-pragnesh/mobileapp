using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public partial class DayOfWeekViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(DayOfWeekViewCell));
        public static readonly UINib Nib;

        static DayOfWeekViewCell()
        {
            Nib = UINib.FromName(nameof(DayOfWeekViewCell), NSBundle.MainBundle);
        }

        protected DayOfWeekViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<DayOfWeekViewCell, SelectableBeginningOfWeekViewModel>();

                bindingSet.Bind(DayOfWeekLabel)
                          .To(vm => vm.BeginningOfWeek);

                bindingSet.Bind(SelectedImageView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.Selected);

                bindingSet.Apply();
            });
        }
    }
}
