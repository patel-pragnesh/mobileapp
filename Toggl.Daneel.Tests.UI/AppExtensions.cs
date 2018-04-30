using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

public static class AppExtensions
{
    public static void PerformBack(this IApp app, string identifier)
    {
        app.Tap(identifier);
    }

    public static void PerformBack(this IApp app, Func<AppQuery, AppQuery> identifier)
    {
        app.Tap(identifier);
    }
}
