﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Toggl.Multivac.Extensions
{
    public static class ReactiveExtensions
    {
        private class Observer<T> : IObserver<T>
        {
            private readonly Action<Exception> onError;
            private readonly Action onCompleted;

            public Observer(Action<Exception> onError, Action onCompleted)
            {
                this.onError = onError;
                this.onCompleted = onCompleted;
            }

            public void OnCompleted()
                => onCompleted();

            public void OnError(Exception error)
                => onError(error);

            public void OnNext(T value) { }
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<Exception> onError, Action onCompleted)
        {
            var observer = new Observer<T>(onError, onCompleted);
            return observable.Subscribe(observer);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<Exception> onError)
        {
            var observer = new Observer<T>(onError, () => { });
            return observable.Subscribe(observer);
        }

        public static IObservable<T> ConnectedReplay<T>(this IObservable<T> observable)
        {
            var replayed = observable.Replay();
            replayed.Connect();
            return replayed;
        }

        public static IObservable<T> DelayIf<T>(this IObservable<T> observable, Predicate<T> predicate, TimeSpan delay)
            => observable.SelectMany(value => predicate(value)
                ? Observable.Return(value).Delay(delay)
                : Observable.Return(value));

        public static void DisposedBy(this IDisposable disposable, CompositeDisposable disposeBag)
        {
            disposeBag.Add(disposable);
        }

        public static IObservable<T> Debug<T>(this IObservable<T> observable, string tag = "")
        => observable.Do(x => Console.WriteLine($"{tag}: {x}"));
    }
}
