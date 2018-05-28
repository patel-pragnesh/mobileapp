using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.Extensions;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views.Reports;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar.QuickSelectShortcuts;
using Toggl.Multivac;
using UIKit;
using Math = System.Math;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ReportsCalendarQuickSelectCollectionViewSource
        : MvxCollectionViewSource, IUICollectionViewDelegateFlowLayout
    {
        private const int cellWidth = 96;
        private const int cellHeight = 32;
        private const string cellIdentifier = nameof(ReportsCalendarQuickSelectViewCell);

        public ReportsCalendarQuickSelectCollectionViewSource(
            UICollectionView collectionView) : base(collectionView)
        {
            Ensure.Argument.IsNotNull(collectionView, nameof(collectionView));

            collectionView.RegisterNibForCell(ReportsCalendarQuickSelectViewCell.Nib, cellIdentifier);
        }

        protected override UICollectionViewCell GetOrCreateCellFor(UICollectionView collectionView, NSIndexPath indexPath, object item)
            => collectionView.DequeueReusableCell(cellIdentifier, indexPath) as UICollectionViewCell;

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(collectionView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            return cell;
        }

        public override nint NumberOfSections(UICollectionView collectionView) => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => ItemsSource.Count();

        protected override object GetItemAt(NSIndexPath indexPath)
            => ItemsSource.ElementAt((int)indexPath.Item);

        [Export("collectionView:layout:sizeForItemAtIndexPath:")]
        public CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(cellWidth, cellHeight);
        }
    }
}
