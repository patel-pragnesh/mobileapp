using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Views;
using MvvmCross.Plugin.Visibility;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [ModalDialogPresentation]
    public sealed partial class SelectColorViewController : MvxViewController<SelectColorViewModel>
    {
        private const int customColorEnabledHeight = 365;
        private const int customColorDisabledHeight = 233;

        public SelectColorViewController()
            : base(nameof(SelectColorViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new ColorSelectionCollectionViewSource(ColorCollectionView);
            prepareViews(source);

            var bindingSet = this.CreateBindingSet<SelectColorViewController, SelectColorViewModel>();

            //Collection View
            bindingSet.Bind(source).To(vm => vm.SelectableColors);
            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.SelectColorCommand);

            //Commands
            bindingSet.Bind(SaveButton).To(vm => vm.SaveCommand);
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);

            bindingSet.Bind(PickerView)
                      .For(v => v.Hue)
                      .To(vm => vm.Hue);

            bindingSet.Bind(PickerView)
                      .For(v => v.Saturation)
                      .To(vm => vm.Saturation);

            bindingSet.Bind(PickerView)
                      .For(v => v.Value)
                      .To(vm => vm.Value);

            bindingSet.Bind(SliderBackgroundView)
                      .For(v => v.Hue)
                      .To(vm => vm.Hue);
            
            bindingSet.Bind(SliderBackgroundView)
                      .For(v => v.Saturation)
                      .To(vm => vm.Saturation);

            bindingSet.Bind(SaveButton)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.AllowCustomColors)
                      .WithConversion(new MvxVisibilityValueConverter());

            bindingSet.Bind(SliderView)
                      .For(v => v.Value)
                      .To(vm => vm.Value)
                      .WithConversion(new ComplementValueConverter());

            bindingSet.Apply();
        }

        private void prepareViews(ColorSelectionCollectionViewSource source)
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Width;
            PreferredContentSize = new CGSize
            {
                // ScreenWidth - 32 for 16pt margins on both sides
                Width = screenWidth > 320 ? screenWidth - 32 : 312,
                Height = ViewModel.AllowCustomColors ? customColorEnabledHeight : customColorDisabledHeight
            };

            ColorCollectionView.Source = source;

            if (!ViewModel.AllowCustomColors) 
            {
                SliderView.Hidden = true;
                PickerView.Hidden = true;
                SliderBackgroundView.Hidden = true;
                return;
            }

            // Initialize picker related layers
            var usableWidth = PreferredContentSize.Width - 28;
            PickerView.InitializeLayers(new CGRect(0, 0, usableWidth, 80));
            SliderBackgroundView.InitializeLayer(new CGRect(0, 0, usableWidth, 18));

            // Remove track
            SliderView.SetMinTrackImage(new UIImage(), UIControlState.Normal);
            SliderView.SetMaxTrackImage(new UIImage(), UIControlState.Normal);
        }
    }
}

