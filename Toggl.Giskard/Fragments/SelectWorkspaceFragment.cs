using System;
using Android.OS;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxFragmentPresentation(typeof(EditProjectViewModel), Resource.Id.SelectWorkspaceContainer, AddToBackStack = true)]
    public sealed class SelectWorkspaceFragment : MvxFragment<SelectWorkspaceViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SelectWorkspaceFragment, null);
            return view;
        }
    }
}
