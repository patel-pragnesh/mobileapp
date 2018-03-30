// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views.Reports
{
	[Register ("ReportsHeaderView")]
	partial class ReportsHeaderView
	{
		[Outlet]
		UIKit.UILabel BillablePercentageLabel { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.PercentageView BillablePercentageView { get; set; }

		[Outlet]
		UIKit.UILabel BillableTitleLabel { get; set; }

		[Outlet]
		UIKit.UIView EmptyStateView { get; set; }

		[Outlet]
		UIKit.UIView LoadingCardView { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.LoadingPieChartView LoadingPieChartView { get; set; }

		[Outlet]
		UIKit.UIView OverviewCardView { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.PieChartView PieChartView { get; set; }

		[Outlet]
		UIKit.UIImageView TotalDurationGraph { get; set; }

		[Outlet]
		UIKit.UILabel TotalDurationLabel { get; set; }

		[Outlet]
		UIKit.UILabel TotalTitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoadingCardView != null) {
				LoadingCardView.Dispose ();
				LoadingCardView = null;
			}

			if (BillablePercentageLabel != null) {
				BillablePercentageLabel.Dispose ();
				BillablePercentageLabel = null;
			}

			if (BillablePercentageView != null) {
				BillablePercentageView.Dispose ();
				BillablePercentageView = null;
			}

			if (BillableTitleLabel != null) {
				BillableTitleLabel.Dispose ();
				BillableTitleLabel = null;
			}

			if (EmptyStateView != null) {
				EmptyStateView.Dispose ();
				EmptyStateView = null;
			}

			if (LoadingPieChartView != null) {
				LoadingPieChartView.Dispose ();
				LoadingPieChartView = null;
			}

			if (OverviewCardView != null) {
				OverviewCardView.Dispose ();
				OverviewCardView = null;
			}

			if (PieChartView != null) {
				PieChartView.Dispose ();
				PieChartView = null;
			}

			if (TotalDurationGraph != null) {
				TotalDurationGraph.Dispose ();
				TotalDurationGraph = null;
			}

			if (TotalDurationLabel != null) {
				TotalDurationLabel.Dispose ();
				TotalDurationLabel = null;
			}

			if (TotalTitleLabel != null) {
				TotalTitleLabel.Dispose ();
				TotalTitleLabel = null;
			}
		}
	}
}
