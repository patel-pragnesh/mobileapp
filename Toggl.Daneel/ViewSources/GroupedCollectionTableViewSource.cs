using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using Toggl.Daneel.Views.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public abstract class GroupedCollectionTableViewSource<TCollection, TItem> : MvxTableViewSource
        where TCollection : MvxObservableCollection<TItem>, new()
        where TItem : class
    {
        private readonly string cellIdentifier;
        private readonly string headerCellIdentifier;
        private readonly object animationLock = new object();
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        private NestableObservableCollection<TCollection, TItem> observableCollection;

        private IList<TCollection> internalCollection;

        public IList<TCollection> GroupedItems => internalCollection;

        public override IEnumerable ItemsSource
        {
            get => internalCollection;
            set { }
        }

        public NestableObservableCollection<TCollection, TItem> ObservableCollection
        {
            get => observableCollection;
            set
            {
                if (observableCollection != null)
                {
                    observableCollection.CollectionChanged -= OnCollectionChanged;
                    observableCollection.OnChildCollectionChanged -= OnChildCollectionChanged;
                }

                observableCollection = value;
                cloneCollection();
                base.ItemsSource = internalCollection;

                if (observableCollection != null)
                {
                    observableCollection.CollectionChanged += OnCollectionChanged;
                    observableCollection.OnChildCollectionChanged += OnChildCollectionChanged;
                }
            }
        }

        protected GroupedCollectionTableViewSource(UITableView tableView, string cellIdentifier, string headerCellIdentifier)
            : base(tableView)
        {
            this.cellIdentifier = cellIdentifier;
            this.headerCellIdentifier = headerCellIdentifier;

            internalCollection = new List<TCollection>();

            UseAnimations = true;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var grouping = GetGroupAt(section);

            var cell = GetOrCreateHeaderViewFor(tableView);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = grouping;

            if (section == 0 && cell is IHeaderViewCellWithHideableTopSeparator headerCell)
                headerCell.TopSeparatorHidden = true;

            return cell;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);

            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
            => ItemsSource.Count();

        public override nint RowsInSection(UITableView tableview, nint section)
            => GetGroupAt(section).Count();

        protected virtual IEnumerable<TItem> GetGroupAt(nint section)
            => internalCollection.ElementAtOrDefault((int)section) ?? new TCollection();

        protected override object GetItemAt(NSIndexPath indexPath)
            => internalCollection.ElementAtOrDefault(indexPath.Section)?.ElementAtOrDefault((int)indexPath.Item);

        public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
        {
            var firstVisible = TableView.IndexPathsForVisibleRows.First();
            if (firstVisible.Section != section + 1) return;

            var nextHeader = TableView.GetHeaderView(firstVisible.Section) as IHeaderViewCellWithHideableTopSeparator;
            if (nextHeader == null) return;

            nextHeader.TopSeparatorHidden = true;
        }

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
        {
            var headerViewCell = headerView as IHeaderViewCellWithHideableTopSeparator;
            if (headerViewCell == null) return;

            var firstVisibleIndexPath = TableView.IndexPathsForVisibleRows.First();
            if (firstVisibleIndexPath.Section == section)
            {
                var nextHeader = TableView.GetHeaderView(section + 1) as IHeaderViewCellWithHideableTopSeparator;
                if (nextHeader == null) return;
                nextHeader.TopSeparatorHidden = false;
                headerViewCell.TopSeparatorHidden = true;
            }
            else
            {
                headerViewCell.TopSeparatorHidden = false;
            }
        }

        protected virtual UITableViewHeaderFooterView GetOrCreateHeaderViewFor(UITableView tableView)
            => tableView.DequeueReusableHeaderFooterView(headerCellIdentifier);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            InvokeOnMainThread(() =>
            {
                if (!UseAnimations)
                {
                    cloneCollection();
                    ReloadTableData();
                    return;
                }

                animateSectionChangesIfPossible(args);
            });
        }

        protected void OnChildCollectionChanged(object sender, ChildCollectionChangedEventArgs args)
        {
            InvokeOnMainThread(() =>
            {
                if (!UseAnimations)
                {
                    cloneCollection();
                    ReloadTableData();
                    return;
                }

                animateRowChangesIfPossible(args);
            });
        }

        private void animateSectionChangesIfPossible(NotifyCollectionChangedEventArgs args)
        {
            lock (animationLock)
            {
                TableView.BeginUpdates();

                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        var indexToAdd = NSIndexSet.FromIndex(args.NewStartingIndex);

                        var addedSection = new TCollection();
                        addedSection.AddRange(observableCollection[args.NewStartingIndex]);

                        internalCollection.Insert(args.NewStartingIndex, addedSection);
                        TableView.InsertSections(indexToAdd, UITableViewRowAnimation.Automatic);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        var indexToRemove = NSIndexSet.FromIndex(args.OldStartingIndex);
                        internalCollection.RemoveAt(args.OldStartingIndex);
                        TableView.DeleteSections(indexToRemove, UITableViewRowAnimation.Automatic);
                        break;

                    case NotifyCollectionChangedAction.Move when args.NewItems.Count == 1 && args.OldItems.Count == 1:
                        internalCollection.RemoveAt(args.OldStartingIndex);

                        var movedSection = new TCollection();
                        movedSection.AddRange(observableCollection[args.NewStartingIndex]);

                        internalCollection.Insert(args.NewStartingIndex, movedSection);
                        TableView.MoveSection(args.OldStartingIndex, args.NewStartingIndex);
                        break;

                    case NotifyCollectionChangedAction.Replace when args.NewItems.Count == args.OldItems.Count:
                        var indexSet = NSIndexSet.FromIndex(args.NewStartingIndex);

                        var replacedSection = new TCollection();
                        replacedSection.AddRange(observableCollection[args.NewStartingIndex]);

                        internalCollection[args.NewStartingIndex] = replacedSection;
                        TableView.ReloadSections(indexSet, ReplaceAnimation);
                        break;

                    default:
                        cloneCollection();
                        TableView.ReloadData();
                        break;
                }

                TableView.EndUpdates();
            }
        }

        private void animateRowChangesIfPossible(ChildCollectionChangedEventArgs args)
        {
            lock (animationLock)
            {
                TableView.BeginUpdates();
                        
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        var indexPathsToAdd = args.Indexes
                            .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                            .ToArray();

                        foreach (var indexPath in indexPathsToAdd)
                            internalCollection[indexPath.Section].Insert(indexPath.Row, observableCollection[indexPath.Section][indexPath.Row]);

                        TableView.InsertRows(indexPathsToAdd, AddAnimation);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        var indexPathsToRemove = args.Indexes
                            .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                            .ToArray();

                        foreach (var indexPath in indexPathsToRemove)
                            internalCollection[indexPath.Section].RemoveAt(indexPath.Row);

                        TableView.DeleteRows(indexPathsToRemove, RemoveAnimation);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        var indexPathsToUpdate = args.Indexes
                            .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                            .ToArray();

                        foreach (var indexPath in indexPathsToUpdate)
                            internalCollection[indexPath.Section][indexPath.Row] = observableCollection[indexPath.Section][indexPath.Row];

                        TableView.ReloadRows(indexPathsToUpdate, ReplaceAnimation);
                        break;

                    default:
                        cloneCollection();
                        TableView.ReloadData();
                        break;
                }

                TableView.EndUpdates();
            }
        }

        private void cloneCollection()
        {
            internalCollection = new List<TCollection>();
            foreach (var section in observableCollection)
            {
                internalCollection.Add(CloneCollection(section));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing || ObservableCollection == null) return;

            ObservableCollection.OnChildCollectionChanged -= OnChildCollectionChanged;
        }

        protected abstract TCollection CloneCollection(TCollection collection);
    }
}
