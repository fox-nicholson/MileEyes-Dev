using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MileEyes.CustomControls.CustomImageCell), typeof(CustomImageCellRenderer))]

namespace MileEyes.iOS.Renderers
{
    class CustomImageCellRenderer : ImageCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var customViewCell = item as ICustomViewCell;

            var cell = base.GetCell(item, reusableCell, tv);

            cell.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 25);

            if (customViewCell.ShowDisclosure)
            {
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }

            cell.SeparatorInset = new UIEdgeInsets(0, 50, 0, 0);

            return cell;
        }
    }
}