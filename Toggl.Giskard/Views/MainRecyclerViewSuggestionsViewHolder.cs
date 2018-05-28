using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross;
using MvvmCross.Platforms.Android;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using TogglResources = Toggl.Foundation.Resources;

namespace Toggl.Giskard.Views
{
    public sealed class MainRecyclerViewSuggestionsViewHolder : MvxRecyclerViewHolder
    {
        public SuggestionsViewModel ViewModel => DataContext as SuggestionsViewModel;

        private TextView hintTextView;
        private TextView indicatorTextView;
        private IDisposable countDisposable;
        private IDisposable currentCardDisposable;
        private static readonly int containerHeightInPixels;

        private int currentSuggestionCard = 1;
        private bool initialized;

        static MainRecyclerViewSuggestionsViewHolder()
        {
            var context = Mvx.Resolve<IMvxAndroidGlobals>().ApplicationContext;
            containerHeightInPixels = 130.DpToPixels(context);
        }

        public MainRecyclerViewSuggestionsViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView, context)
        {
        }

        public MainRecyclerViewSuggestionsViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            initializeIfNeeded();
        }

        private void initializeIfNeeded()
        {
            if (initialized) return;

            hintTextView = ItemView.FindViewById<TextView>(Resource.Id.SuggestionsHintTextView);
            indicatorTextView = ItemView.FindViewById<TextView>(Resource.Id.SuggestionsIndicatorTextView);

            countDisposable =
                Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    e => ViewModel.Suggestions.CollectionChanged += e,
                    e => ViewModel.Suggestions.CollectionChanged -= e)
                .Select(args => ViewModel.Suggestions.Count)
                .StartWith(ViewModel.Suggestions.Count)
                .DistinctUntilChanged()
                .Subscribe(onCollectionCountChanged);

            currentCardDisposable =
                ItemView.FindViewById<SuggestionsRecyclerView>(Resource.Id.SuggestionsRecyclerView)
                    .CurrentIndexObservable.Subscribe(onCurrentSuggestionIndexChanged);

            initialized = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            currentCardDisposable?.Dispose();
            countDisposable?.Dispose();
        }

        private void onCurrentSuggestionIndexChanged(int currentIndex)
        {
            currentSuggestionCard = currentIndex;
            updateHintText();
        }

        private void onCollectionCountChanged(int itemCount)
        {
            updateHintText();
        }

        private void updateHintText()
        {
            var numberOfSuggestions = ViewModel.Suggestions.Count;

            switch (numberOfSuggestions)
            {
                case 0:
                    return;
                
                case 1:
                    hintTextView.Text = TogglResources.WorkingOnThis;
                    indicatorTextView.Visibility = ViewStates.Gone;
                    break;

                default:
                    var indicatorText = $"{currentSuggestionCard} {TogglResources.Of.ToUpper()} {numberOfSuggestions}";
                    hintTextView.Text = TogglResources.WorkingOnThese;
                    indicatorTextView.Visibility = ViewStates.Visible;
                    indicatorTextView.Text = indicatorText;
                    break;
            }

        }
    }
}
