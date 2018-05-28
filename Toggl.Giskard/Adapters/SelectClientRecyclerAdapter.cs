using System;
using System.Windows.Input;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.TemplateSelectors;

namespace Toggl.Giskard.Adapters
{
    public sealed class SelectClientRecyclerAdapter : SelectCreatableEntityRecyclerAdapter
    {
        public SelectClientRecyclerAdapter()
        {
        }

        public SelectClientRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override string SuggestingItemText
            => $"Create client \"{Text.Trim()}\"";

        protected override ICommand GetClickCommand(int viewType) =>
            viewType == SelectClientTemplateSelector.CreateEntity ? CreateCommand : ItemClick;
    }
}
