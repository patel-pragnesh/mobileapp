using System;
using System.Reactive.Disposables;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using System.Reactive;

namespace Toggl.Foundation.MvvmCross.Views
{
    public interface ISettingsView
    {
        void LoggingOut();
        void OnEmailChanged(string email);
        void OnDurationChanged(string duration);
        void OnDateFormatChanged(string dateFormat);
        void OnManualModeChanged(bool isOnManualMode);
        void OnWorkspaceNameChanged(string workspaceName);
        void OnBeginningOfWeekChanged(string beginningOfWeek);
        void OnUseTwentyFourHourFormatChanged(bool useTwentyFourHourFormat);

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
    }

    public static class ISettingsViewExtensions
    {
        public static IDisposable CreateBindings(this ISettingsView view, SettingsViewModel viewModel)
        {
            var disposeBag = new CompositeDisposable();

            viewModel.Email.Subscribe(view.OnEmailChanged).DisposedBy(disposeBag);
            viewModel.Duration.Subscribe(view.OnDurationChanged).DisposedBy(disposeBag);
            viewModel.WorkspaceName.Subscribe(view.OnWorkspaceNameChanged).DisposedBy(disposeBag);
            viewModel.CurrentDateFormat.Subscribe(view.OnDateFormatChanged).DisposedBy(disposeBag);
            viewModel.IsManualModeEnabled.Subscribe(view.OnManualModeChanged).DisposedBy(disposeBag);
            viewModel.BeginningOfWeek.Subscribe(view.OnBeginningOfWeekChanged).DisposedBy(disposeBag);
            viewModel.UseTwentyFourHourFormat.Subscribe(view.OnUseTwentyFourHourFormatChanged).DisposedBy(disposeBag);

            view.EmailTappedObservable.Subscribe((Unit _) => viewModel.EditProfile()).DisposedBy(disposeBag);
            view.AboutTappedObservable.Subscribe((Unit _) => viewModel.OpenAboutPage()).DisposedBy(disposeBag);
            view.ManualModeTappedObservable.Subscribe((Unit _) => viewModel.ToggleManualMode()).DisposedBy(disposeBag);
            view.LogoutButtonTappedObservable.Subscribe(async (Unit _) => await viewModel.TryLogout()).DisposedBy(disposeBag);
            view.WorkspaceTappedObservable.Subscribe(async (Unit _) => await viewModel.PickWorkspace()).DisposedBy(disposeBag);
            view.FeedbackTappedObservable.Subscribe(async (Unit _) => await viewModel.SubmitFeedback()).DisposedBy(disposeBag);
            view.DateFormatTappedObservable.Subscribe(async (Unit _) => await viewModel.SelectDateFormat()).DisposedBy(disposeBag);
            view.DurationFormatTappedObservable.Subscribe(async (Unit _) => await viewModel.SelectDurationFormat()).DisposedBy(disposeBag);
            view.BeginningOfWeekTappedObservable.Subscribe(async (Unit _) => await viewModel.SelectBeginningOfWeek()).DisposedBy(disposeBag);
            view.TwentyFourHourClockTappedObservable.Subscribe(async (Unit _) => await viewModel.ToggleUseTwentyFourHourClock()).DisposedBy(disposeBag);

            return disposeBag;
        }
    }
}
