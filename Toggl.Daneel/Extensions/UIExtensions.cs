using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class UIExtensions
    {
        public static IObservable<Unit> Tapped(this UIButton button)
            => Observable
                .FromEventPattern(e => button.TouchUpInside += e, e => button.TouchUpInside -= e)
                .SelectUnit();

        public static IObservable<Unit> Tapped(this UIView view)
            => Observable.Create<Unit>(observer =>
            {
                var gestureRecognizer = new UITapGestureRecognizer(() => observer.OnNext(Unit.Default));
                gestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
                view.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() => view.RemoveGestureRecognizer(gestureRecognizer));
            });

        public static IObservable<DateTimeOffset> DateChanged(this UIDatePicker datePicker)
            => Observable
                .FromEventPattern(e => datePicker.ValueChanged += e, e => datePicker.ValueChanged -= e)
                .Select(e => ((UIDatePicker) e.Sender).Date.ToDateTimeOffset());

        public static IObservable<DateTimeOffset> DateComponentChanged(this UIDatePicker datePicker)
            => datePicker.DateChanged()
                .StartWith(datePicker.Date.ToDateTimeOffset())
                .DistinctUntilChanged(d => d.Date)
                .Skip(1);

        public static IObservable<DateTimeOffset> TimeComponentChanged(this UIDatePicker datePicker)
            => datePicker.DateChanged()
                .StartWith(datePicker.Date.ToDateTimeOffset())
                .DistinctUntilChanged(d => d.TimeOfDay)
                .Skip(1);

        public static Action<bool> BindIsVisible(this UIView view)
            => isVisible => view.Hidden = !isVisible;

        public static Action<bool> BindIsVisibleWithFade(this UIView view)
            => isVisible =>
            {
                var alpha = isVisible ? 1 : 0;
                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.EaseIn,
                    () => view.Alpha = alpha
                );
            };

        public static Action<string> BindText(this UILabel label)
            => text => label.Text = text;

        public static Action<NSAttributedString> BindAttributedText(this UILabel label)
            => text => label.AttributedText = text;

        public static Action<string> BindText(this UITextView textView)
            => text => textView.Text = text;

        public static Action<string> BindText(this UITextField textField)
            => text => textField.Text = text;

        public static Action<bool> BindIsOn(this UISwitch @switch)
            => isOn => @switch.SetState(isOn, true);

        public static Action<string> BindTitle(this UIButton button)
            => title => button.SetTitle(title, UIControlState.Normal);

        public static Action<nfloat> BindConstant(this NSLayoutConstraint constraint)
            => constant => constraint.Constant = constant;
    }
}
