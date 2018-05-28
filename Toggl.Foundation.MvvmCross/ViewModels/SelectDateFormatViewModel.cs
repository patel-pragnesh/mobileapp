using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectDateFormatViewModel : MvxViewModel<DateFormat, DateFormat>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly DateFormat[] availableDateFormats =
        {
            DateFormat.FromLocalizedDateFormat("MM/DD/YYYY"),
            DateFormat.FromLocalizedDateFormat("DD-MM-YYYY"),
            DateFormat.FromLocalizedDateFormat("MM-DD-YYYY"),
            DateFormat.FromLocalizedDateFormat("YYYY-MM-DD"),
            DateFormat.FromLocalizedDateFormat("DD/MM/YYYY"),
            DateFormat.FromLocalizedDateFormat("DD.MM.YYYY")
        };

        private DateFormat defaultResult;

        public SelectableDateFormatViewModel[] DateTimeFormats { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<SelectableDateFormatViewModel> SelectFormatCommand { get; }

        public SelectDateFormatViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectFormatCommand = new MvxAsyncCommand<SelectableDateFormatViewModel>(selectFormat);

            DateTimeFormats = availableDateFormats
                .Select(dateFormat => new SelectableDateFormatViewModel(dateFormat, false))
                .ToArray();
        }

        public override void Prepare(DateFormat parameter)
        {
            defaultResult = parameter;

            updateSelectedFormat(parameter);
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task selectFormat(SelectableDateFormatViewModel dateFormatViewModel)
            => navigationService.Close(this, dateFormatViewModel.DateFormat);

        private void updateSelectedFormat(DateFormat selected)
            => DateTimeFormats.ForEach(dateFormat
                => dateFormat.Selected = dateFormat.DateFormat == selected);
    }
}
