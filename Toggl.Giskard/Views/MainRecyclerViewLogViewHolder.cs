using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Views
{
    public sealed class MainRecyclerViewLogViewHolder : MvxRecyclerViewHolder
    {
        private Button continueButton;
        private bool continueClickOverloaded;

        public bool CanSync { get; set; }

        public View ContinueBackground { get; set; }
        public View DeleteBackground { get; set; }
        public View ContentView { get; set; }

        private IMvxAsyncCommand<TimeEntryViewModel> continueCommand;
        public IMvxAsyncCommand<TimeEntryViewModel> ContinueCommand 
        {
            get => continueCommand;
            set 
            {
                if (continueCommand == value) return;

                continueCommand = value;
                if (continueCommand == null) return;

                ensureContinueClickOverloaded();
            }
        }

        public MainRecyclerViewLogViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView, context)
        {
        }

        public MainRecyclerViewLogViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        private void ensureContinueClickOverloaded()
        {
            if (continueClickOverloaded) return;
            continueButton = ItemView.FindViewById<Button>(Resource.Id.TimeEntriesLogCellContinueButton);
            continueButton.Click += onContinueButtonClick;
            continueClickOverloaded = true;
        }

        private void onContinueButtonClick(object sender, EventArgs e)
        {
            ExecuteCommandOnItem(ContinueCommand);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            if (continueButton == null) return;
            continueButton.Click -= onContinueButtonClick;
        }
    }
}
