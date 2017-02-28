using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomJourneyCell), typeof(CustomJourneyCellRenderer))]

namespace MileEyes.iOS.Renderers
{
    public class CustomJourneyCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);

            cell.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 25);

            cell.SeparatorInset = new UIEdgeInsets(0, 35, 0, 0);

            return cell;
        }
    }
}