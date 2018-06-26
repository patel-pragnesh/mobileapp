using System.Reactive.Disposables;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Toggl.Foundation.MvvmCross.Parameters.SelectTimeParameters.Origin;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme")]
    public sealed partial class EditTimeEntryActivity : MvxAppCompatActivity<EditTimeEntryViewModel>, IReactiveBindingHolder
    {
        public CompositeDisposable DisposeBag { get; private set; } = new CompositeDisposable();

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));
            
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditTimeEntryActivity);

            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);

            initializeViews();
            setupBindings();
        }

        private void setupBindings()
        {
            this.Bind(startTimeArea.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(StartTime));
            this.Bind(stopTimeArea.Tapped(), _ => ViewModel.StopTimeEntryCommand.Execute(StopTime));
            this.Bind(durationArea.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(Duration));
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.CloseCommand.ExecuteAsync();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            DisposeBag?.Dispose();
        }
    }
}
