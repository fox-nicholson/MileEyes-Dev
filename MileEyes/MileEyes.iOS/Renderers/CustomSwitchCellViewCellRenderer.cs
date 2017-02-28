using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DefaultInvoicedViewCell), typeof(CustomSwitchCellViewCellRenderer))]
[assembly: ExportRenderer(typeof(InvoicedViewCell), typeof(CustomSwitchCellViewCellRenderer))]
[assembly: ExportRenderer(typeof(CloudSyncViewCell), typeof(CustomSwitchCellViewCellRenderer))]

namespace MileEyes.iOS.Renderers
{
    class CustomSwitchCellViewCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);

            cell.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 25);

            cell.SeparatorInset = new UIEdgeInsets(0, 50, 0, 0);

            return cell;
        }
    }
}