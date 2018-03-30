using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class SelectDateTimeFragment : MvxDialogFragment<SelectDateTimeViewModel>
    {
        private DateTimeOffset dateTime;

        public SelectDateTimeFragment()
        {
        }

        public SelectDateTimeFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            switch (ViewModel.Mode)
            {
                case DateTimePickerMode.Date:
                    return showDate();
                case DateTimePickerMode.Time:
                    return showTime();
                default:
                    throw new NotSupportedException();
            }
        }

        private DatePickerDialog showDate()
        {
            var localTime = ViewModel.CurrentDateTime.ToLocalTime();

            var dialog = new DatePickerDialog(
                Activity, onDateSet,
                localTime.Year, localTime.Month - 1, localTime.Day);

            dialog.DatePicker.MinDate = ViewModel.MinDate.ToUnixTimeMilliseconds();
            dialog.DatePicker.MaxDate = ViewModel.MaxDate.ToUnixTimeMilliseconds();

            return dialog;
        }

        private TimePickerDialog showTime()
        {
            var localTime = ViewModel.CurrentDateTime.ToLocalTime();

            var dialog = new TimePickerDialog(
                Activity, onTimeSet,
                localTime.Hour, localTime.Minute, ViewModel.Is24HoursFormat);

            return dialog;
        }

        private void onDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            var localTime = ViewModel.CurrentDateTime.ToLocalTime();

            dateTime = new DateTimeOffset(e.Year, e.Month + 1, e.DayOfMonth,
                                          localTime.Hour, localTime.Minute, localTime.Second,
                                          localTime.Offset);
        }

        private void onTimeSet(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            var localTime = ViewModel.CurrentDateTime.ToLocalTime();

            dateTime = new DateTimeOffset(localTime.Year, localTime.Month, localTime.Day,
                                          e.HourOfDay, e.Minute, localTime.Second,
                                          localTime.Offset);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CloseCommand.Execute();
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            if (ViewModel == null) return;

            ViewModel.CurrentDateTime = dateTime;
            ViewModel.SaveCommand.Execute();
        }
    }
}
