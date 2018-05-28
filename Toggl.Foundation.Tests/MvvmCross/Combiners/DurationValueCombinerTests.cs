using System;
using FluentAssertions;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.Combiners
{
    public sealed class DurationValueCombinerTests
    {
        private static readonly DurationValueCombiner combiner = new DurationValueCombiner();

        private static bool tryGetValue(TimeSpan duration, object format, out object convertedValue)
        {
            var durationStepDescription = new MvxLiteralSourceStepDescription();
            durationStepDescription.Literal = duration;

            var formatStepDescription = new MvxLiteralSourceStepDescription();
            formatStepDescription.Literal = format;

            var steps = new[]
            {
                new MvxLiteralSourceStep(durationStepDescription),
                new MvxLiteralSourceStep(formatStepDescription)
            };

            return combiner.TryGetValue(steps, out convertedValue);
        }

        public sealed class TheTryGetValueMethod
        {
            public sealed class InvalidFormats
            {
                [Theory, LogIfTooSlow]
                [InlineData(null)]
                [InlineData((int)DurationFormat.Classic)]
                [InlineData("Classic")]
                [InlineData(false)]
                public void FailsForFormatsWhichAreNotDurationFormat(object parameter)
                {
                    var successful = tryGetValue(TimeSpan.Zero, parameter, out var convertedValue);

                    successful.Should().BeFalse();
                    convertedValue.Should().Be(MvxBindingConstant.UnsetValue);
                }
            }

            public sealed class ValidFormats
            {
                [Theory, LogIfTooSlow]
                [InlineData(DurationFormat.Classic)]
                [InlineData(DurationFormat.Improved)]
                [InlineData(DurationFormat.Decimal)]
                public void FailsForDurationFormatsWhichAreOutOfRange(DurationFormat format)
                {
                    var successful = tryGetValue(TimeSpan.Zero, format, out var convertedValue);

                    successful.Should().BeTrue();
                    convertedValue.Should().BeOfType<string>();
                    ((string)convertedValue).Length.Should().BeGreaterThan(0);
                }
            }
        }
    }
}
