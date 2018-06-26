using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Toggl.Daneel.Views;
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

        public static IObservable<string> Text(this UITextField textField)
            => Observable
                .FromEventPattern(handler => textField.EditingChanged += handler, handler => textField.EditingChanged -= handler)
                .Select(_ => textField.Text);

        public static IObservable<bool> FirstResponder(this LoginTextField textField)
            => Observable
                .FromEventPattern(handler => textField.IsFirstResponderChanged += handler, handler => textField.IsFirstResponderChanged -= handler)
                .Select(_ => textField.IsFirstResponder)
                .StartWith(false)
                .DistinctUntilChanged();

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

        public static Action<UIColor> BindTintColor(this UIView view)
            => color => view.TintColor = color;

        public static Action<UIColor> BindTitleColor(this UIButton button)
            => color => button.SetTitleColor(color, UIControlState.Normal);

        public static Action<bool> BindAnimatedIsVisible(this UIView view)
            => isVisible =>
            {
                if (view.Hidden != isVisible) return;

                view.Transform = CGAffineTransform.MakeTranslation(0, 20);

                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.SharpCurve,
                    () =>
                    {
                        view.Hidden = !isVisible;
                        view.Transform = CGAffineTransform.MakeTranslation(0, 0);
                    }
                );
            };

        public static Action<string> BindAnimatedTitle(this UIButton button)
            => title =>
            {
                UIView.Transition(
                    button,
                    Animation.Timings.EnterTiming,
                    UIViewAnimationOptions.TransitionCrossDissolve,
                    () => button.SetTitle(title, UIControlState.Normal),
                    null
                );
            };

        public static (Action<bool>, IDisposable) BindSecureTextEntry(this UITextField textField)
        {
            void onEditingDidBegin(object sender, EventArgs e)
            {
                if (!textField.SecureTextEntry) return;
                textField.InsertText(textField.Text);
            }

            textField.EditingDidBegin += onEditingDidBegin;

            Action<bool> onNext = isSecure =>
            {
                if (textField.SecureTextEntry == isSecure) return;

                textField.ResignFirstResponder();
                textField.SecureTextEntry = isSecure;
                textField.BecomeFirstResponder();
            };
            var disposable = Disposable.Create(() => textField.EditingDidBegin -= onEditingDidBegin);

            return (onNext, disposable);
        }
    }
}
