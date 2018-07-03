using System;
using Foundation;
using UIKit;
using Toggl.Foundation.Models;
using static Toggl.PrimeRadiant.SyncStatus;

namespace Toggl.Daneel.Cells
{
    public partial class SyncFailureCell : BaseTableViewCell<SyncFailureItem>
    {
        public static readonly string Identifier = "syncFailureCell";

        public static readonly NSString Key = new NSString("SyncFailureCell");
        public static readonly UINib Nib;

        static SyncFailureCell()
        {
            Nib = UINib.FromName("SyncFailureCell", NSBundle.MainBundle);
        }

        protected SyncFailureCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        protected override void UpdateView()
        {
            nameLabel.Text = Item.Name;
            typeLabel.Text = $"{Item.Type}: ";
            errorMessageLabel.Text = Item.SyncErrorMessage;
            syncStatusLabel.Text = Item.SyncStatus.ToString();

            switch (Item.SyncStatus) 
            {
                case SyncFailed:
                    syncStatusLabel.TextColor = UIColor.Red;
                    break;
                case SyncNeeded:
                    syncStatusLabel.TextColor = UIColor.DarkGray;
                    break;
            }
        }
    }
}
