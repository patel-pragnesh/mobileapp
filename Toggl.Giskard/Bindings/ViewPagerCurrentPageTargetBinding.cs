using System;
using Android.Graphics.Drawables;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;
using Toggl.Giskard.Extensions;
using MvvmCross.Plugin.Color.Platforms.Android;
using MvvmCross.UI;
using Android.Graphics;
using Android.Support.V4.View;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Object = Java.Lang.Object;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewPagerCurrentPageTargetBinding : MvxAndroidTargetBinding<ViewPager, int>
    {
        public const string BindingName = "CurrentPage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        private readonly IDisposable disposable;
        private readonly OnPageChangeListener pageChangeListener = new OnPageChangeListener();

        public ViewPagerCurrentPageTargetBinding(ViewPager target) : base(target)
        {
            target.AddOnPageChangeListener(pageChangeListener);
            disposable = pageChangeListener.PageChangeObservable.Subscribe(FireValueChanged);
        }

        protected override void SetValueImpl(ViewPager target, int value)
        {
            target.SetCurrentItem(value, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            disposable?.Dispose();
            Target?.ClearOnPageChangeListeners();
        }

        private class OnPageChangeListener : Object, ViewPager.IOnPageChangeListener
        {
            private readonly Subject<int> pageChangedSubject = new Subject<int>();

            public IObservable<int> PageChangeObservable { get; }

            public OnPageChangeListener()
            {
                PageChangeObservable = pageChangedSubject.AsObservable();
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
            }

            public void OnPageScrollStateChanged(int state)
            {
            }

            public void OnPageSelected(int position)
            {
                pageChangedSubject.OnNext(position);
            }
        }
    }
}
