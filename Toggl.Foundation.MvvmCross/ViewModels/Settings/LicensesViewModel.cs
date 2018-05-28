using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class LicensesViewModel : MvxViewModel
    {
        private readonly ILicenseProvider licenseProvider;

        public List<License> Licenses { get; private set; }

        public LicensesViewModel(ILicenseProvider licenseProvider)
        {
            Ensure.Argument.IsNotNull(licenseProvider, nameof(licenseProvider));

            this.licenseProvider = licenseProvider;
        }

        public override Task Initialize()
        {
            Licenses = licenseProvider
                .GetAppLicenses()
                .Select(keyValuePair => new License(keyValuePair.Key, keyValuePair.Value))
                .ToList();

            return base.Initialize();
        }
    }
}
