using Android.Content;
using Android.Util;

namespace Toggl.Giskard.Extensions
{
    public static class NumberExtensions
    {
        public static float DpToPixels(this int self, Context context)
            => ((float)self).DpToPixels(context);

        public static float DpToPixels(this float self, Context context)
        => TypedValue.ApplyDimension(ComplexUnitType.Dip, self, context.Resources.DisplayMetrics);

        public static float SpToPixels(this int self, Context context)
            => ((float)self).SpToPixels(context);

        public static float SpToPixels(this float self, Context context)
            => TypedValue.ApplyDimension(ComplexUnitType.Sp, self, context.Resources.DisplayMetrics);
    }
}
