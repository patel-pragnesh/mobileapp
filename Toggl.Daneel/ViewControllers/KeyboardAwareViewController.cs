using MvvmCross.ViewModels;
using MvvmCross.iOS.Views;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public abstract class KeyboardAwareViewController<TViewModel> : MvxViewController<TViewModel>
        where TViewModel : class, IMvxViewModel
    {
        public KeyboardAwareViewController(string nibName)
            : base(nibName, null) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UIKeyboard.Notifications.ObserveWillShow(KeyboardWillShow);
            UIKeyboard.Notifications.ObserveWillHide(KeyboardWillHide);
        }

        protected abstract void KeyboardWillShow(object sender, UIKeyboardEventArgs e);

        protected abstract void KeyboardWillHide(object sender, UIKeyboardEventArgs e);
    }
}
