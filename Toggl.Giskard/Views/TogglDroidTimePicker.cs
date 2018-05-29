using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Helper;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using JavaBool = Java.Lang.Boolean;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.togglDroidTimePicker")]
    public class TogglDroidTimePicker
        : TimePicker
        , TimePicker.IOnTimeChangedListener
    {
        private bool isInitialized;

        public TogglDroidTimePicker(Context context)
            : base(context)
        {
        }

        public TogglDroidTimePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected TogglDroidTimePicker(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public TimeSpan Value
        {
            get
            {
                if (Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
                #pragma warning disable CS0618
                    return new TimeSpan((int)CurrentHour, (int)CurrentMinute, 0);
                else
                    return new TimeSpan(Hour, Minute, 0);
            }
            set
            {
                if (isInitialized)
                {
                    SetOnTimeChangedListener(this);
                    isInitialized = true;
                }

                if (Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
                {
                    #pragma warning disable CS0618
                    if ((int)CurrentHour != value.Hours)
                    {
                        CurrentHour = (Java.Lang.Integer)value.Hours;
                    }
                    if ((int)CurrentMinute != value.Minutes)
                    {
                        CurrentMinute = (Java.Lang.Integer)value.Minutes;
                    }
                }
                else
                {
                    if (Hour != value.Hours)
                    {
                        Hour = value.Hours;
                    }
                    if (Minute != value.Minutes)
                    {
                        Minute = value.Minutes;
                    }
                }
            }
        }

        public event EventHandler ValueChanged;

        public void Update24HourMode(bool is24HourMode)
        {
            SetIs24HourView(new JavaBool(is24HourMode));
        }

        public void OnTimeChanged(TimePicker view, int hourOfDay, int minute)
        {
            ValueChanged?.Invoke(this, null);
        }
    }
}