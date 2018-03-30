using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class SelectDateFormatFragment : MvxDialogFragment<SelectDateFormatViewModel>
    {
        public SelectDateFormatFragment() { }

        public SelectDateFormatFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base (javaReference, transfer) { }
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SelectDateFormatFragment, null);
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            var displayMetrics = new DisplayMetrics();
            Activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            var screenWidth = displayMetrics.WidthPixels;
            var isLargeScreen = screenWidth > 360.DpToPixels(Context);

            var width = (int)(isLargeScreen ? screenWidth - 72.DpToPixels(Context) : 312.DpToPixels(Context));
            var height = (int)400.DpToPixels(Context);

            Dialog.Window.SetLayout(width, height);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CloseCommand.ExecuteAsync();
        }
    }
}
