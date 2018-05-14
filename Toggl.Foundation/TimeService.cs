using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Toggl.Foundation
{
    public sealed class TimeService : ITimeService
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly IScheduler scheduler;

        public DateTimeOffset CurrentDateTime => floor(scheduler.Now);

        public IObservable<DateTimeOffset> CurrentDateTimeObservable { get; }

        public IObservable<DateTimeOffset> MidnightObservable { get; }

        public TimeService(IScheduler scheduler)
        {
            this.scheduler = scheduler;

            CurrentDateTimeObservable =
                Observable
                    .Interval(TimeSpan.FromSeconds(1), scheduler)
                    .Select(_ => CurrentDateTime)
                    .Publish().RefCount();

            var localTimeObservable = CurrentDateTimeObservable
                .Select(t => t.ToLocalTime());

            MidnightObservable = localTimeObservable
                .Zip(localTimeObservable.Skip(1), (previous, now) => (previous: previous, now: now))
                .Where(t => t.previous.Date != t.now.Date)
                .Select(t => t.now);
        }

        private DateTimeOffset floor(DateTimeOffset t)
            => new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, t.Offset);
    }
}
