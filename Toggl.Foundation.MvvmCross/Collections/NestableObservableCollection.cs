using System;
using System.Collections.Specialized;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.Collections
{
    public class NestableObservableCollection<TCollection, TItem> : MvxObservableCollection<TCollection>
        where TCollection : MvxObservableCollection<TItem>
    {
        private readonly Func<TItem, Func<TItem, bool>> innerCollectionOrderFunction;

        public event EventHandler<ChildCollectionChangedEventArgs> OnChildCollectionChanged;

        public NestableObservableCollection()
            : this(a => b => true)
        {
        }

        public NestableObservableCollection(Func<TItem, Func<TItem, bool>> innerCollectionOrderFunction)
        {
            this.innerCollectionOrderFunction = innerCollectionOrderFunction;
        }

        public void InsertInChildCollection(int childCollectionIndex, TItem item)
        {
            var collection = Items[childCollectionIndex];

            var possibleIndex = collection.IndexOf(innerCollectionOrderFunction(item));
            var actualIndex = possibleIndex < 0 ? collection.Count : possibleIndex;
            collection.Insert(actualIndex, item);

            raiseChildCollectionChanged(NotifyCollectionChangedAction.Add, childCollectionIndex, actualIndex);
        }

        public void RemoveFromChildCollection(int childCollectionIndex, TItem item)
        {
            var collection = Items[childCollectionIndex];
            var itemCount = collection.Count;

            if (itemCount == 1)
            {
                RemoveItem(childCollectionIndex);
            }
            else
            {
                var removedIndex = collection.IndexOf(item);
                collection.Remove(item);
                raiseChildCollectionChanged(NotifyCollectionChangedAction.Remove, childCollectionIndex, removedIndex);
            }
        }

        public void ReplaceInChildCollection(int childCollectionIndex, int index, TItem item)
        {
            Items[childCollectionIndex][index] = item;
            raiseChildCollectionChanged(NotifyCollectionChangedAction.Replace, childCollectionIndex, index);
        }

        private void raiseChildCollectionChanged(NotifyCollectionChangedAction action, int collectionIndex, int index)
        {
            raiseChildCollectionChanged(action, collectionIndex, new[] { index });
        }

        private void raiseChildCollectionChanged(NotifyCollectionChangedAction action, int collectionIndex, int[] indexes)
        {
            var args = new ChildCollectionChangedEventArgs(action, collectionIndex, indexes);
            OnChildCollectionChanged?.Invoke(this, args);
        }
    }

    public sealed class ChildCollectionChangedEventArgs : EventArgs
    {
        public int CollectionIndex { get; }

        public int[] Indexes { get; }

        public NotifyCollectionChangedAction Action { get; }

        public ChildCollectionChangedEventArgs(NotifyCollectionChangedAction action, int collectionIndex, int[] indexes)
        {
            Action = action;
            Indexes = indexes;
            CollectionIndex = collectionIndex;
        }
    }
}
