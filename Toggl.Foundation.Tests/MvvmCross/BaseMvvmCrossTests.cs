using MvvmCross.Core;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.Platform;
using MvvmCross.Test.Core;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Shortcuts;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public abstract class BaseMvvmCrossTests
    {
        protected IIdProvider IdProvider { get; } = Substitute.For<IIdProvider>();
        protected ITimeService TimeService { get; } = Substitute.For<ITimeService>();
        protected ITogglDataSource DataSource { get; } = Substitute.For<ITogglDataSource>();
        protected IUserPreferences UserPreferences { get; } = Substitute.For<IUserPreferences>();
        protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
        protected IInteractorFactory InteractorFactory { get; } = Substitute.For<IInteractorFactory>();
        protected IApplicationShortcutCreator ApplicationShortcutCreator { get; }
            = Substitute.For<IApplicationShortcutCreator>();

        protected IMvxIoCProvider Ioc { get; private set; }

        protected IMvxNavigationService NavigationService { get; } = Substitute.For<IMvxNavigationService>();

        static BaseMvvmCrossTests()
        {
            if (MvxSingleton<IMvxIoCProvider>.Instance != null) return;

            MvxSingletonCache.Initialize();
            MvxSimpleIoCContainer.Initialize();

            MvxSingleton<IMvxIoCProvider>.Instance.RegisterSingleton<IMvxTrace>(new TestTrace());
            MvxSingleton<IMvxIoCProvider>.Instance.RegisterSingleton<IMvxSettings>(new MvxSettings());
            
            MvxTrace.Initialize();
        }

        protected BaseMvvmCrossTests()
        {
            Ioc = MvxSingleton<IMvxIoCProvider>.Instance;
        }
    }
}
