using System;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using UIKit;
using static Toggl.Multivac.Extensions.FunctionalExtensions;

namespace Toggl.Daneel.Services
{
    public sealed class DialogService : IDialogService
    {
        private readonly ITopViewControllerProvider topViewControllerProvider;

        public DialogService(ITopViewControllerProvider topViewControllerProvider)
        {
            Ensure.Argument.IsNotNull(topViewControllerProvider, nameof(topViewControllerProvider));
            
            this.topViewControllerProvider = topViewControllerProvider;
        }

        public Task<bool> Confirm(
            string title,
            string message,
            string confirmButtonText,
            string dismissButtonText)
        {
            var tcs = new TaskCompletionSource<bool>();

            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            var confirm = UIAlertAction.Create(confirmButtonText, UIAlertActionStyle.Default, _ => tcs.SetResult(true));
            var dismiss = UIAlertAction.Create(dismissButtonText, UIAlertActionStyle.Cancel, _ => tcs.SetResult(false));

            alert.AddAction(confirm);
            alert.AddAction(dismiss);
            alert.PreferredAction = confirm;

            topViewControllerProvider
                .TopViewController
                .PresentViewController(alert, true, null);

            return tcs.Task;
        }

        public Task<bool> ConfirmDestructiveAction(ActionType type)
        {
            var tcs = new TaskCompletionSource<bool>();
            var actionSheet = UIAlertController.Create(
                title: null,
                message: null,
                preferredStyle: UIAlertControllerStyle.ActionSheet
            );

            var (confirmText, cancelText) = selectTextByType(type);

            var cancelAction = UIAlertAction.Create(cancelText, UIAlertActionStyle.Cancel, _ => tcs.SetResult(false));
            actionSheet.AddAction(cancelAction);

            actionSheet.AddAction(
                UIAlertAction.Create(
                    confirmText,
                    UIAlertActionStyle.Destructive,
                    _ => tcs.SetResult(true)));

            topViewControllerProvider
                .TopViewController
                .PresentViewController(actionSheet, true, null);

            return tcs.Task;
        }

        public Task Alert(string title, string message, string buttonTitle)
        {
            var tcs = new TaskCompletionSource<object>();

            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            var alertAction = UIAlertAction.Create(buttonTitle, UIAlertActionStyle.Default, _ => tcs.SetResult(null));

            alert.AddAction(alertAction);

            topViewControllerProvider
                .TopViewController
                .PresentViewController(alert, true, null);

            return tcs.Task;
        }

        private (string, string) selectTextByType(ActionType type)
        {
            switch (type)
            {
                case ActionType.DiscardNewTimeEntry:
                    return (Resources.Discard, Resources.Cancel);
                case ActionType.DiscardEditingChanges:
                    return (Resources.Discard, Resources.ContinueEditing);
                case ActionType.DeleteExistingTimeEntry:
                    return (Resources.Delete, Resources.Cancel);
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
