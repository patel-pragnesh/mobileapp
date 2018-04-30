using Android.Content;
using Android.Net;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Foundation.MvvmCross.Services;

namespace Toggl.Giskard.Services
{
    public sealed class BrowserService : MvxAndroidTask, IBrowserService
    {
        public void OpenStore()
        {
            OpenUrl("https://play.google.com/store/apps/details?id=com.toggl.giskard");
        }

        public void OpenUrl(string url)
        {
            var intent = new Intent(Intent.ActionView).SetData(Uri.Parse(url));
            StartActivity(intent);
        }
    }
}
