using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectDurationFormatViewModel : MvxViewModel<DurationFormat, DurationFormat>
    {
        private readonly IMvxNavigationService navigationService;

        private DurationFormat defaultResult;

        public SelectableDurationFormatViewModel[] DurationFormats { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<SelectableDurationFormatViewModel> SelectDurationFormatCommand { get; }

        public SelectDurationFormatViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectDurationFormatCommand = new MvxAsyncCommand<SelectableDurationFormatViewModel>(selectFormat);

            DurationFormats = Enum.GetValues(typeof(DurationFormat))
                            .Cast<DurationFormat>()
                            .Select(DurationFormat => new SelectableDurationFormatViewModel(DurationFormat, false))
                            .ToArray();
        }

        public override void Prepare(DurationFormat parameter)
        {
            defaultResult = parameter;
            updateSelectedFormat(parameter);
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task selectFormat(SelectableDurationFormatViewModel viewModel)
            => navigationService.Close(this, viewModel.DurationFormat);

        private void updateSelectedFormat(DurationFormat selected)
            => DurationFormats.ForEach(viewModel
                => viewModel.Selected = viewModel.DurationFormat == selected);
    }
}
