using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class SelectColorFragment : MvxDialogFragment<SelectColorViewModel>
    {
        private int customColorEnabledHeight;
        private int customColorDisabledHeight;

        public SelectColorFragment() { }

        public SelectColorFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base (javaReference, transfer) { }
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SelectColorFragment, null);

            view.FindViewById<RecyclerView>(Resource.Id.SelectColorRecyclerView)
                .SetLayoutManager(new GridLayoutManager(Context, 5));

            customColorEnabledHeight = 425.DpToPixels(Context);
            customColorDisabledHeight = 270.DpToPixels(Context);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            var height = ViewModel.AllowCustomColors ? customColorEnabledHeight : customColorDisabledHeight;

            Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: height);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CloseCommand.ExecuteAsync();
        }
    }
}
