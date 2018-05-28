using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Converters;
using Toggl.Foundation.Extensions;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Combiners
{
    [Preserve(AllMembers = true)]
    public sealed class DurationValueCombiner : MvxValueCombiner
    {
        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out object value)
        {
            TimeSpan duration;
            DurationFormat format;

            try
            {
                (duration, format) = getParameters(steps);
            }
            catch (ArgumentException)
            {
                value = MvxBindingConstant.UnsetValue;
                return false;
            }

            value = duration.ToFormattedString(format);
            return true;
        }

        private (TimeSpan, DurationFormat) getParameters(IEnumerable<IMvxSourceStep> steps)
        {
            var parameters = steps.ToArray();

            if (parameters.Length != 2)
                throw new ArgumentException(
                    $"The {nameof(DurationValueCombiner)} must be used with 2 parameters: a TimeSpan and a DurationFormat. It was instead used with {parameters.Length} parameters.");

            var duration = parameters[0].GetValue() as TimeSpan?;
            if (!duration.HasValue)
                throw new ArgumentException($"The first parameter of the {nameof(DurationValueCombiner)} must be a {nameof(TimeSpan)}, but the given parameter is of type {parameters[0].SourceType.FullName}");

            var format = parameters[1].GetValue() as DurationFormat?;
            if (!format.HasValue)
                throw new ArgumentException($"The second parameter of the {nameof(DurationValueCombiner)} must be a {nameof(DurationFormat)}, but the given parameter is of type {parameters[1].SourceType.FullName}");

            return (duration.Value, format.Value);
        }
    }
}
