using Android.App;
using Android.Content;
using Android.Net;
using Android.Service.QuickSettings;

namespace Toggl.Giskard.Tiles
{
    [Service(Name = "toggl.giskard.Tiles.NewTimeEntryTileService", 
             Label = "@string/NewTimeEntry", 
             Icon = "@drawable/play",
             Permission = "android.permission.BIND_QUICK_SETTINGS_TILE")]
    [IntentFilter(new [] { "android.service.quicksettings.action.QS_TILE" })]
    public sealed class NewTimeEntryTileService : TileService
    {
        public override void OnClick()
        {
            base.OnClick();

            var intent = new Intent(Intent.ActionView).SetData(Uri.Parse("toggl://start"));
            StartActivityAndCollapse(intent); 
        }
    }
}
