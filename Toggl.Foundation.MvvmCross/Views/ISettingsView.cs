using System;
using System.Reactive.Disposables;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.PrimeRadiant.Models;
using Toggl.Multivac.Extensions;
using System.Reactive;

namespace Toggl.Foundation.MvvmCross.Views
{
    public interface ISettingsView
    {
        void UserChanged(IDatabaseUser user);
        void LoggingOut()

        IObservable<Unit> EmailTappedObservable { get; }
        IObservable<Unit> AboutTappedObservable { get; }
        IObservable<Unit> FeedbackTappedObservable { get; }
        IObservable<Unit> WorkspaceTappedObservable { get; }
        IObservable<Unit> ManualModeTappedObservable { get; }
        IObservable<Unit> DateFormatTappedObservable { get; }
        IObservable<Unit> LogoutButtonTappedObservable { get; }
        IObservable<Unit> DurationFormatTappedObservable { get; }
        IObservable<Unit> BeginningOfWeekTappedObservable { get; }
        IObservable<Unit> TwentyFourHourClockTappedObservable { get; }
        IObservable<Unit> TwentyFourHourClockSwTappedObservable { get; }
    }

    public static class ISettingsViewExtensions
    {
        public static IDisposable CreateBindings(this ISettingsView view, SettingsViewModel viewModel)
        {
            CompositeDisposable disposeBag = new CompositeDisposable();

            viewModel.CurrentUser.Subscribe(view.UserChanged).DisposedBy(disposeBag);

            view.EmailTappedObservable.Subscribe(viewModel.EditProfile).DisposedBy(disposeBag);
            view.AboutTappedObservable.Subscribe(viewModel.OpenAboutPage).DisposedBy(disposeBag);
            view.LogoutButtonTappedObservable.Subscribe(viewModel.TryLogout).DisposedBy(disposeBag);
            view.WorkspaceTappedObservable.Subscribe(viewModel.PickWorkspace).DisposedBy(disposeBag);
            view.FeedbackTappedObservable.Subscribe(viewModel.SubmitFeedback).DisposedBy(disposeBag);
            view.ManualModeTappedObservable.Subscribe(viewModel.ToggleManualMode).DisposedBy(disposeBag);
            view.DateFormatTappedObservable.Subscribe(viewModel.SelectDateFormat).DisposedBy(disposeBag);
            view.DurationFormatTappedObservable.Subscribe(viewModel.SelectDurationFormat).DisposedBy(disposeBag);
            view.BeginningOfWeekTappedObservable.Subscribe(viewModel.SelectBeginningOfWeek).DisposedBy(disposeBag);
            view.TwentyFourHourClockTappedObservable.Subscribe(viewModel.ToggleUseTwentyFourHourClock).DisposedBy(disposeBag);
            view.TwentyFourHourClockSwTappedObservable.Subscribe(viewModel.ToggleUseTwentyFourHourClock).DisposedBy(disposeBag);

            //// Text
            //bindingSet.Bind(WorkspaceLabel).To(vm => vm.WorkspaceName);
            //bindingSet.Bind(DateFormatLabel).To(vm => vm.DateFormat.Localized);
            //bindingSet.Bind(DurationFormatLabel)
            //          .To(vm => vm.DurationFormat)
            //          .WithConversion(durationFormatToStringConverter);
            //bindingSet.Bind(BeginningOfWeekLabel).To(vm => vm.BeginningOfWeek);
            //bindingSet.Bind(VersionLabel).To(vm => vm.Version);


            //// Logout process
            //bindingSet.Bind(LogoutButton)
            //          .For(btn => btn.Enabled)
            //          .To(vm => vm.IsLoggingOut)
            //          .WithConversion(inverseBoolConverter);

            //bindingSet.Bind(NavigationItem)
            //          .For(nav => nav.BindHidesBackButton())
            //          .To(vm => vm.IsLoggingOut);

            //bindingSet.Bind(SyncingView)
            //          .For(view => view.BindVisibility())
            //          .To(vm => vm.IsRunningSync)
            //          .WithConversion(visibilityConverter);

            //bindingSet.Bind(SyncedView)
            //          .For(view => view.BindVisibility())
            //          .To(vm => vm.IsSynced)
            //          .WithConversion(visibilityConverter);

            //bindingSet.Bind(LoggingOutView)
            //          .For(view => view.BindVisibility())
            //          .To(vm => vm.IsLoggingOut)
            //          .WithConversion(visibilityConverter);

            //// Switches
            //bindingSet.Bind(TwentyFourHourClockSwitch)
            //          .For(v => v.BindAnimatedOn())
            //          .To(vm => vm.UseTwentyFourHourClock);

            //bindingSet.Bind(ManualModeSwitch)
            //          .For(v => v.BindAnimatedOn())
            //          .To(vm => vm.IsManualModeEnabled);

            //bindingSet.Apply();


            return disposeBag;
        }
    }
}
