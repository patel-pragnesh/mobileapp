using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ColorSelectionCollectionViewSource : MvxCollectionViewSource, IUICollectionViewDelegateFlowLayout
    {
        private const string cellIdentifier = nameof(ColorSelectionViewCell);

        public ColorSelectionCollectionViewSource(UICollectionView collectionView)
            : base (collectionView)
        {
            collectionView.RegisterNibForCell(ColorSelectionViewCell.Nib, cellIdentifier);
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(collectionView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            return cell;
        }

        protected override UICollectionViewCell GetOrCreateCellFor(UICollectionView collectionView, NSIndexPath indexPath, object item)
            => collectionView.DequeueReusableCell(cellIdentifier, indexPath) as UICollectionViewCell;

        [Export("collectionView:layout:sizeForItemAtIndexPath:")]
        public CGSize GetSizeForItem(
            UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
            => new CGSize(Math.Floor(collectionView.Frame.Width / 5), 36);

        [Export("collectionView:layout:minimumLineSpacingForSectionAtIndex:")]
        public nfloat GetMinimumLineSpacingForSection(
            UICollectionView collectionView, UICollectionViewLayout layout, nint section)
            => 12;

        [Export("collectionView:layout:minimumInteritemSpacingForSectionAtIndex:")]
        public nfloat GetMinimumInteritemSpacingForSection(
            UICollectionView collectionView, UICollectionViewLayout layout, nint section)
            => 0;

        [Export("collectionView:layout:insetForSectionAtIndex:")]
        public UIEdgeInsets GetInsetForSection(
            UICollectionView collectionView, UICollectionViewLayout layout, nint section)
            => new UIEdgeInsets(0, 0, 0, 0);
    }
}
