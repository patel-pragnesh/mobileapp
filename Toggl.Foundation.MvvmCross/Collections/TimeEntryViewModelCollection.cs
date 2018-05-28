using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Collections
{
    [Preserve(AllMembers = true)]
    public sealed class TimeEntryViewModelCollection : MvxObservableCollection<TimeEntryViewModel>
    {
        public DateTimeOffset Date { get; }

        public TimeSpan TotalTime { get; private set; }

        public DurationFormat DurationFormat { get; set; }

        public TimeEntryViewModelCollection()
        {
        }

        public TimeEntryViewModelCollection(DateTime date, IEnumerable<TimeEntryViewModel> items, DurationFormat durationFormat)
        {
            Ensure.Argument.IsNotNull(items, nameof(items));

            if (date.Kind != DateTimeKind.Local)
                throw new ArgumentException($"{nameof(date)} must have kind DateTimeKind.Local");

            this.AddRange(items);

            Date = new DateTimeOffset(date);
            TotalTime = calculateTotalTime();
            DurationFormat = durationFormat;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            TotalTime = calculateTotalTime();
        }

        protected override void SetItem(int index, TimeEntryViewModel item)
        {
            base.SetItem(index, item);
            TotalTime = calculateTotalTime();
        }

        protected override void InsertItem(int index, TimeEntryViewModel item)
        {
            base.InsertItem(index, item);
            TotalTime += item.Duration ?? TimeSpan.Zero;
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            TotalTime = calculateTotalTime();
        }

        private TimeSpan calculateTotalTime()
        => Items.Aggregate(TimeSpan.Zero, (acc, vm) => acc + (vm.Duration ?? TimeSpan.Zero));
    }
}
