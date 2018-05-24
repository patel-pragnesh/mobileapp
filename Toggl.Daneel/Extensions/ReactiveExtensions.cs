using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class ReactiveExtensions
    {
        public static IObservable<Unit> TappedObservable(this UIButton button)
            => button.Events().TouchUpInside.Select(_ => Unit.Default);

        public static IObservable<Unit> TappedObservable(this UIView view)
            => Observable.Create<Unit>(observer =>
            {
                var gestureRecognizer = new UITapGestureRecognizer(() => observer.OnNext(Unit.Default));
                view.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() => view.RemoveGestureRecognizer(gestureRecognizer));
            });
    }
}
