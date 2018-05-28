using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Support.V4.App;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Views.Base;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Login;

namespace Toggl.Giskard.Services
{
    public sealed class GoogleService : MvxAndroidTask, IGoogleService
    {
        private const int googleSignInResult = 123;
        private readonly object lockable = new object();

        private bool isLoggingIn;
        private Subject<string> subject = new Subject<string>();

        public IObservable<string> GetAuthToken()
        {
            lock (lockable)
            {
                if (isLoggingIn)
                    return subject.AsObservable();

                isLoggingIn = true;
                subject = new Subject<string>();

                var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity as FragmentActivity;

                var signInOptions = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken("{TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID}")
                    .RequestEmail()
                    .Build();

                var googleApiClient = new GoogleApiClient.Builder(activity)
                    .EnableAutoManage(activity, onError)
                    .AddApi(Auth.GOOGLE_SIGN_IN_API, signInOptions)
                    .Build();

                var intent = Auth.GoogleSignInApi.GetSignInIntent(googleApiClient);
                StartActivityForResult(googleSignInResult, intent);

                return subject.AsObservable();
            }
        }

        protected override void ProcessMvxIntentResult(MvxIntentResultEventArgs result)
        {
            base.ProcessMvxIntentResult(result);

            if (result.RequestCode != googleSignInResult)
                return;

            lock(lockable)
            {
                var signInData = Auth.GoogleSignInApi.GetSignInResultFromIntent(result.Data);
                if (signInData.IsSuccess)
                {
                    subject.OnNext(signInData.SignInAccount.IdToken);
                    subject.OnCompleted();
                }
                else
                {
                    subject.OnError(new GoogleLoginException(signInData.Status.IsCanceled));
                }

                isLoggingIn = false;
            }
        }

        private void onError(ConnectionResult result)
        {
            lock (lockable)
            {
                subject.OnError(new GoogleLoginException(false));
                isLoggingIn = false;
            }
        }
    }
}
