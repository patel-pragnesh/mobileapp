using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Plugin.Color;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class ColorSelectionViewCell : MvxCollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ColorSelectionViewCell));
        public static readonly UINib Nib;

        static ColorSelectionViewCell()
        {
            Nib = UINib.FromName(nameof(ColorSelectionViewCell), NSBundle.MainBundle);
        }

        protected ColorSelectionViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            SelectedImageView.Image = SelectedImageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            SelectedImageView.TintColor = UIColor.White;

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<ColorSelectionViewCell, SelectableColorViewModel>();

                bindingSet.Bind(ColorCircleView)
                          .For(v => v.BackgroundColor)
                          .To(vm => vm.Color)
                          .WithConversion(new MvxNativeColorValueConverter());

                bindingSet.Bind(SelectedImageView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.Selected);

                bindingSet.Apply();
            });
        }
    }
}
