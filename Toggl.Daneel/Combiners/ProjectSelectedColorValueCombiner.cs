using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Plugin.Color;
using UIKit;

namespace Toggl.Daneel.Combiners
{
    public sealed class ProjectSelectedColorValueCombiner : MvxValueCombiner
    {
        private readonly nfloat alpha;
        private readonly MvxRGBValueConverter colorConverter = new MvxRGBValueConverter();

        public ProjectSelectedColorValueCombiner(nfloat alpha)
        {
            this.alpha = alpha;
        }

        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out object value)
        {
            var stepArray = steps.ToArray();
            if (stepArray.Length < 2)
                throw new ArgumentException($"{nameof(steps)} has to have 2 arguments");

            var isSelected = (bool)stepArray[0].GetValue();
            var projectColor = (string)stepArray[1].GetValue();

            var color = (UIColor)colorConverter.Convert(projectColor, typeof(UIColor), null, CultureInfo.CurrentCulture);

            value = isSelected ? color.ColorWithAlpha(alpha) : UIColor.Clear;

            return true;
        }
    }
}
