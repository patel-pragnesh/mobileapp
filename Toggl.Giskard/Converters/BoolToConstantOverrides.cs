using Android.Graphics;
using Android.Support.V4.Graphics;
using MvvmCross.Plugins.Color.Droid;
using Toggl.Foundation.MvvmCross.Converters;
using static Toggl.Foundation.MvvmCross.Helper.Color;

// Use this file to register overrides of BoolToConstantValueConverter.
// They are all single line classes, but an override is needed to avoid using the Fluent syntax.
namespace Toggl.Giskard.Converters
{
    public sealed class LogDescriptionTopOffsetValueConverter : BoolToConstantValueConverter<int>
    {
        public LogDescriptionTopOffsetValueConverter() : base(16, 27) { }
    }

    public sealed class ProjectDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public ProjectDrawableValueConverter() : base(Resource.Drawable.te_project_active, Resource.Drawable.project) { }
    }

    public sealed class TagsDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public TagsDrawableValueConverter() : base(Resource.Drawable.te_tag_active, Resource.Drawable.tag) { }
    }

    public sealed class BillableDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public BillableDrawableValueConverter() : base(Resource.Drawable.te_billable_active, Resource.Drawable.billable) { }
    }

    public sealed class FontWeightValueConverter : BoolToConstantValueConverter<TypefaceStyle>
    {
        public FontWeightValueConverter() : base(TypefaceStyle.Bold, TypefaceStyle.Normal) { }
    }

    public sealed class EditProjectErrorOffsetValueConverter : BoolToConstantValueConverter<int>
    {
        public EditProjectErrorOffsetValueConverter() : base(8, 14) { }
    }

    public sealed class CreateProjectButtonColorValueConverter : BoolToConstantValueConverter<Color>
    {
        public CreateProjectButtonColorValueConverter()
            : base(Color.ParseColor("#328fff"), new Color(ColorUtils.SetAlphaComponent(Color.ParseColor("#328fff").ToArgb(), 127))) { }
    }

    public sealed class ReportsChartColorValueConverter : BoolToConstantValueConverter<Color>
    {
        public ReportsChartColorValueConverter()
            : base(Reports.Disabled.ToAndroidColor(), Reports.TotalTimeActivated.ToAndroidColor()) { }
    }

    public sealed class LoginInfoTextColorValueConverter : BoolToConstantValueConverter<Color>
    {
        public LoginInfoTextColorValueConverter()
            : base(Color.ParseColor("#e20505"), Color.ParseColor("#5e5b5b")) { }
    }
    
    public sealed class ManualModeEnabledDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public ManualModeEnabledDrawableValueConverter() : base(Resource.Drawable.add_white, Resource.Drawable.play_white) { }
    }
}
