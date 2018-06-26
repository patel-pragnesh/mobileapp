using System;
using UIKit;

namespace Toggl.Daneel.Cells
{
    public abstract class BaseTableViewCell<T> : UITableViewCell
    {
        private T item;
        public T Item
        {
            get => item;
            set
            {
                item = value;
                UpdateView();
            }
        }

        protected BaseTableViewCell()
        {
        }

        protected BaseTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        protected abstract void UpdateView();
    }
}
