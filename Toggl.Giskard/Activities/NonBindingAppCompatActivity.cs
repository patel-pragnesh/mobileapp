using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat.EventSource;

namespace Toggl.Giskard.Activities
{
    public class NonBindingAppCompatActivity<TViewModel> : MvxEventSourceAppCompatActivity
        where TViewModel : class, IMvxViewModel 
    {
        public object DataContext { get; set; }

        public TViewModel ViewModel
        {
            get => DataContext as TViewModel;
            set => DataContext = value;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel?.ViewCreated();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel?.ViewDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            ViewModel?.ViewAppearing();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ViewModel?.ViewAppeared();
        }

        protected override void OnPause()
        {
            base.OnPause();
            ViewModel?.ViewDisappearing();
        }

        protected override void OnStop()
        {
            base.OnStop();
            ViewModel?.ViewDisappeared();
        }
    }
}
