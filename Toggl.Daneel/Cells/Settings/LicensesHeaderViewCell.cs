using System;
using Foundation;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.Cells.Settings
{
    public sealed partial class LicensesHeaderViewCell : BaseTableHeaderFooterView<License>
    {
        public static readonly NSString Key = new NSString(nameof(LicensesHeaderViewCell));
        public static readonly UINib Nib;

        static LicensesHeaderViewCell()
        {
            Nib = UINib.FromName(nameof(LicensesHeaderViewCell), NSBundle.MainBundle);
        }

        public LicensesHeaderViewCell(IntPtr handle) : base(handle)
        {
        }

        protected override void UpdateView()
        {
            HeaderLabel.Text = Item.Subject;
        }
    }
}
