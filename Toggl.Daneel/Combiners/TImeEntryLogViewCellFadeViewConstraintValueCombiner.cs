using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;

namespace Toggl.Daneel.Combiners
{
    public sealed class TimeEntryLogViewCellFadeViewConstraintValueCombiner : MvxValueCombiner
    {
        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out object value)
        {
            var stepArray = steps.ToArray();

            if (stepArray.Length < 2)
                throw new ArgumentException($"{nameof(steps)} must have 2 values");

            var isBillable = (bool)stepArray[0].GetValue();
            var hasTags = (bool)stepArray[1].GetValue();

            if (isBillable && hasTags)
            {
                value = (nfloat)136;
            }
            else if (isBillable || hasTags)
            {
                value = (nfloat)112;
            }
            else
            {
                value = (nfloat)96;
            }

            return true;
        }
    }
}
