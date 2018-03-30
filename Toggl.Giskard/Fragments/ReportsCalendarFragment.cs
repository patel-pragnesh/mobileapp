using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxFragmentPresentation(typeof(ReportsViewModel), Resource.Id.ReportsCalendarContainer, AddToBackStack = false)]
    public sealed class ReportsCalendarFragment : MvxFragment<ReportsCalendarViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.ReportsCalendarFragment, null);

            Mvx.Resolve<IMvxBindingContextStack<IMvxAndroidBindingContext>>()
               .Push(BindingContext as IMvxAndroidBindingContext);

            var pager = view.FindViewById<ViewPager>(Resource.Id.ReportsCalendarFragmentViewPager);
            pager.Adapter = new CalendarPagerAdapter(Activity, ViewModel);
            pager.SetCurrentItem(ViewModel.Months.Count - 1, false);

            view.FindViewById<LinearLayout>(Resource.Id.ReportsCalendarFragmentHeader)
                .GetChildren<TextView>()
                .Indexed()
                .ForEach((textView, index)
                    => textView.Text = ViewModel.DayHeaderFor(index));

            return view;
        }
    }
}
