using System.Threading.Tasks;

namespace Toggl.Foundation.MvvmCross.Services
{
    public interface IDialogService
    {
        Task<bool> Confirm(
            string title,
            string message,
            string confirmButtonText,
            string dismissButtonText);

        Task Alert(string title, string message, string buttonTitle);

        Task<bool> ConfirmDestructiveAction(ActionType type);
    }

    public enum ActionType
    {
        DiscardNewTimeEntry,
        DiscardEditingChanges,
        DeleteExistingTimeEntry
    }
}
