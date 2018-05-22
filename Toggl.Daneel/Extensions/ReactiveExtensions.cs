using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class ReactiveExtensions
    {
        public static IObservable<Unit> TappedObservable(this UIButton button)
            => button.Events().TouchUpInside.Select(_ => Unit.Default);

        public static IObservable<Unit> TappedObservable(this UIView view)
        {
            var subject = new Subject<Unit>();
            var gestureRecognizer = new UIGestureRecognizer(() => subject.OnNext(Unit.Default));
            view.AddGestureRecognizer(gestureRecognizer);

            return Observable.Create<Unit>(observer =>
            {
                subject.AsObservable().Subscribe(observer);

                return Disposable.Create(() => view.RemoveGestureRecognizer(gestureRecognizer));
            });
        }
    }
}
