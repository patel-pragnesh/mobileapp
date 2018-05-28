using System;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectDateTimeViewModel : MvxViewModel<DateTimePickerParameters, DateTimeOffset>
    {
        private DateTimeOffset defaultResult;

        private readonly IMvxNavigationService navigationService;

        private DateTimeOffset currentDateTime;
        public DateTimeOffset CurrentDateTime
        {
            get => currentDateTime;
            set
            {
                var newValue = createDateTimeBasedOnMode(value);
                if (currentDateTime == newValue) return;

                currentDateTime = newValue.Clamp(MinDate, MaxDate);

                RaisePropertyChanged(nameof(CurrentDateTime));
            }
        }

        public bool Is24HoursFormat { get; private set; } = true;

        public DateTimeOffset MinDate { get; private set; }

        public DateTimeOffset MaxDate { get; private set; }
       
        public DateTimePickerMode Mode { get; private set; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand SaveCommand { get; set; }

        public SelectDateTimeViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SaveCommand = new MvxAsyncCommand(save);
        }

        public override void Prepare(DateTimePickerParameters parameter)
        {
            Mode = parameter.Mode;
            MinDate = parameter.MinDate;
            MaxDate = parameter.MaxDate;
            CurrentDateTime = defaultResult = parameter.CurrentDate;
        }

        private DateTimeOffset createDateTimeBasedOnMode(DateTimeOffset dateTime)
        {
            switch (Mode)
            {
                case DateTimePickerMode.Date:
                    return defaultResult.ToUniversalTime().WithDate(dateTime.ToUniversalTime());
                
                case DateTimePickerMode.Time:
                    return defaultResult.ToUniversalTime().WithTime(dateTime.ToUniversalTime());

                case DateTimePickerMode.DateTime:
                    return dateTime;
                
                default:
                    throw new NotSupportedException("Invalid DateTimePicker mode");
            }
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task save() => navigationService.Close(this, CurrentDateTime);
    }
}
