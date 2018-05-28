using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using MvvmCross.UI;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using MvvmCross.Commands;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectColorViewModel : MvxViewModel<ColorParameters, MvxColor>
    {
        private readonly IMvxNavigationService navigationService;

        private MvxColor defaultColor;
        private readonly SelectableColorViewModel customColor =
            new SelectableColorViewModel(MvxColors.Transparent, false);

        public float Hue { get; set; } = 0.0f;

        public float Saturation { get; set; } = 0.0f;

        public float Value { get; set; } = 0.375f;

        public bool AllowCustomColors { get; private set; }

        public IMvxAsyncCommand SaveCommand { get; set; }

        public IMvxAsyncCommand CloseCommand { get; set; }

        public IMvxCommand<SelectableColorViewModel> SelectColorCommand { get; set; }

        public MvxObservableCollection<SelectableColorViewModel> SelectableColors { get; } =
            new MvxObservableCollection<SelectableColorViewModel>();

        public SelectColorViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            SaveCommand = new MvxAsyncCommand(save);
            CloseCommand = new MvxAsyncCommand(close);
            SelectColorCommand = new MvxCommand<SelectableColorViewModel>(selectColor);
        }

        public override void Prepare(ColorParameters parameter)
        {
            defaultColor = parameter.Color;
            AllowCustomColors = parameter.AllowCustomColors;

            SelectableColors.AddRange(
                Color.DefaultProjectColors.Select(color => new SelectableColorViewModel(color, color == defaultColor))
            );

            var noColorsSelected = SelectableColors.All(color => !color.Selected);
            if (AllowCustomColors)
            {
                SelectableColors.Add(customColor);
                if (noColorsSelected)
                {
                    customColor.Selected = true;
                    customColor.Color = defaultColor;

                    (Hue, Saturation, Value) = defaultColor.GetHSV();
                }
                else
                {
                    customColor.Color = Color.FromHSV(Hue, Saturation, Value);
                    
                }
            }
            else if (noColorsSelected)
            {
                SelectableColors.First().Selected = true;
            }
        }

        private void OnHueChanged()
        {
            updateColor();
        }

        private void OnSaturationChanged()
        {
            updateColor();
        }

        private void OnValueChanged()
        {
            updateColor();
        }

        private void updateColor()
        {
            customColor.Color = Color.FromHSV(Hue, Saturation, Value);
            selectColor(customColor);
        }

        private void selectColor(SelectableColorViewModel color)
        {
            foreach (var selectableColor in SelectableColors)
                selectableColor.Selected = color.Color.ARGB == selectableColor.Color.ARGB;

            if (AllowCustomColors) return;
            save();
        }

        private Task close()
            => navigationService.Close(this, defaultColor);

        private Task save()
            => navigationService.Close(this, SelectableColors.FirstOrDefault(x => x.Selected).Color);
    }
}
