using System;
using Toggl.Tests.UI;
using Xamarin.UITest;

public static class AppExtensions
{
    public static void PerformBack(this IApp app, string identifier)
    {
        switch (identifier)
        {
            // When at the login screen, the keyboard is up. 
            // This means going back requires two back presses.
            case Login.BackButton:
                app.Back();
                break;
        }

        app.Back();
    }
}
