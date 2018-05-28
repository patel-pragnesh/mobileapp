using Toggl.Foundation.MvvmCross.Combiners;
using MvvmCross.Platforms.Android.Binding;
using System;
using MvvmCross.Binding.Combiners;

namespace Toggl.Giskard
{
    public sealed class TogglBindingBuilder : MvxAndroidBindingBuilder
    {
        protected override void FillValueCombiners(IMvxValueCombinerRegistry registry)
        {
            registry.AddOrOverwrite("Duration", new DurationValueCombiner());
            registry.AddOrOverwrite("DateTimeOffsetShortDateFormat", new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Local, false));
            registry.AddOrOverwrite("DateTimeOffsetTimeFormat", new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Local));
            base.FillValueCombiners(registry);
        }
    }
}