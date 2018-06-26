using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class LoginViewModel : MvxViewModel<CredentialsParameter>
    {
        private readonly ILoginManager loginManager;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;
        private readonly IPasswordManagerService passwordManagerService;
        private readonly IErrorHandlingService errorHandlingService;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly ITimeService timeService;

        private IDisposable loginDisposable;

        private readonly BehaviorSubject<Email> emailSubject = new BehaviorSubject<Email>(Multivac.Email.Empty);
        private readonly BehaviorSubject<Password> passwordSubject = new BehaviorSubject<Password>(Multivac.Password.Empty);
        private readonly BehaviorSubject<string> errorMessageSubject = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<bool> isPasswordMaskedSubject = new BehaviorSubject<bool>(true);
        private readonly Subject<bool> isShowPasswordButtonVisibleSubject = new Subject<bool>();

        public IObservable<Email> Email { get; }

        public IObservable<Password> Password { get; }

        public IObservable<string> ErrorMessage { get; }

        public IObservable<bool> HasError { get; }

        public IObservable<bool> IsLoading { get; }

        public IObservable<bool> LoginEnabled { get; }

        public IObservable<bool> IsPasswordManagerAvailable { get; }

        public IObservable<bool> IsPasswordMasked { get; }

        public IObservable<bool> IsShowPasswordButtonVisible { get; }

        public LoginViewModel(
            ILoginManager loginManager,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IPasswordManagerService passwordManagerService,
            IErrorHandlingService errorHandlingService,
            ILastTimeUsageStorage lastTimeUsageStorage,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(passwordManagerService, nameof(passwordManagerService));
            Ensure.Argument.IsNotNull(errorHandlingService, nameof(errorHandlingService));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.loginManager = loginManager;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;
            this.passwordManagerService = passwordManagerService;
            this.errorHandlingService = errorHandlingService;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.timeService = timeService;

            Email = emailSubject.AsObservable();
            Password = passwordSubject.AsObservable();
            IsLoading = isLoadingSubject.AsObservable();
            ErrorMessage = errorMessageSubject.AsObservable();
            IsPasswordMasked = isPasswordMaskedSubject.AsObservable();
            IsShowPasswordButtonVisible = Password
                .Select(password => password.Length > 1)
                .CombineLatest(
                    isShowPasswordButtonVisibleSubject.AsObservable(),
                    (passwordIsLongEnough, valueFromSubject) => passwordIsLongEnough && valueFromSubject);

            HasError = ErrorMessage.Select(error => !string.IsNullOrEmpty(error));
            LoginEnabled = Email
                .CombineLatest(
                    Password,
                    IsLoading,
                    (email, password, isLoading) => email.IsValid && password.IsValid && !isLoading);
            IsPasswordManagerAvailable = Observable.Create((IObserver<bool> observer) =>
            {
                observer.OnNext(passwordManagerService.IsAvailable);
                return Disposable.Empty;
            });
        }

        public override void Prepare(CredentialsParameter parameter)
        {
            emailSubject.OnNext(parameter.Email);
            passwordSubject.OnNext(parameter.Password);
        }

        public void SetEmail(Email email)
            => emailSubject.OnNext(email);

        public void SetPassword(Password password)
            => passwordSubject.OnNext(password);

        public void SetIsShowPasswordButtonVisible(bool visible)
            => isShowPasswordButtonVisibleSubject.OnNext(visible);

        public void Login()
        {
            var emailInvalid = !emailSubject.Value.IsValid;
            var passwordInvalid = !passwordSubject.Value.IsValid;

            if (emailInvalid || passwordInvalid)
            {
                var hint = new ShakeAuthenticationFieldHint(emailInvalid, passwordInvalid);
                navigationService.ChangePresentation(hint);
                return;
            }

            isLoadingSubject.OnNext(true);
            errorMessageSubject.OnNext("");

            loginDisposable =
                loginManager
                    .Login(emailSubject.Value, passwordSubject.Value)
                    .Track(analyticsService.Login, AuthenticationMethod.EmailAndPassword)
                    .Subscribe(onDataSource, onError, onCompleted);
        }

        private async void onDataSource(ITogglDataSource dataSource)
        {
            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            await dataSource.StartSyncing();

            isLoadingSubject.OnNext(false);

            onboardingStorage.SetIsNewUser(false);

            await navigationService.Navigate<MainViewModel>();
        }

        private void onError(Exception exception)
        {
            isLoadingSubject.OnNext(false);
            onCompleted();

            if (errorHandlingService.TryHandleDeprecationError(exception))
                return;

            switch (exception)
            {
                case UnauthorizedException forbidden:
                    errorMessageSubject.OnNext(Resources.IncorrectEmailOrPassword);
                    break;
                case GoogleLoginException googleEx when googleEx.LoginWasCanceled:
                    errorMessageSubject.OnNext("");
                    break;
                default:
                    errorMessageSubject.OnNext(Resources.GenericLoginError);
                    break;
            }
        }

        private void onCompleted()
        {
            loginDisposable?.Dispose();
            loginDisposable = null;
        }
        public async Task StartPasswordManager()
        {
            analyticsService.PasswordManagerButtonClicked.Track();

            var loginInfo = await passwordManagerService.GetLoginInformation();

            emailSubject.OnNext(loginInfo.Email);
            if (!emailSubject.Value.IsValid) return;
            analyticsService.PasswordManagerContainsValidEmail.Track();

            passwordSubject.OnNext(loginInfo.Password);
            if (!passwordSubject.Value.IsValid) return;
            analyticsService.PasswordManagerContainsValidPassword.Track();

            Login();
        }

        public void TogglePasswordVisibility()
            => isPasswordMaskedSubject.OnNext(!isPasswordMaskedSubject.Value);

        public async Task ForgotPassword()
        {
            if (isLoadingSubject.Value) return;

            var emailParameter = EmailParameter.With(emailSubject.Value);
            emailParameter = await navigationService
                .Navigate<ForgotPasswordViewModel, EmailParameter, EmailParameter>(emailParameter);
            if (emailParameter != null)
                emailSubject.OnNext(emailParameter.Email);
        }

        public void GoogleLogin()
        {
            if (isLoadingSubject.Value) return;

            isLoadingSubject.OnNext(true);

            loginDisposable = loginManager
                .LoginWithGoogle()
                .Track(analyticsService.Login, AuthenticationMethod.Google)
                .Subscribe(onDataSource, onError, onCompleted);
        }

        public Task Signup()
        {
            if (isLoadingSubject.Value)
                return Task.CompletedTask;

            var parameter = CredentialsParameter.With(emailSubject.Value, passwordSubject.Value);
            return navigationService.Navigate<SignupViewModel, CredentialsParameter>(parameter);
        }
    }
}
