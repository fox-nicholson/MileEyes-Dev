using System.Linq;
using Foundation;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomEntryCell), typeof(CustomEntryCellRenderer))]

namespace MileEyes.iOS.Renderers
{
    class CustomEntryCellRenderer : EntryCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var textCell = item as CustomEntryCell;

            var cell = base.GetCell(item, reusableCell, tv);

            cell.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 25);

            var subviews = cell.Subviews;

            if (textCell == null) return cell;

            subviews.FirstOrDefault().Subviews.FirstOrDefault(sv => sv is UILabel).BackgroundColor = UIColor.Clear;

            var inputColor = textCell.LabelColor.ToUIColor().ColorWithAlpha(0.7f);
            (subviews.FirstOrDefault().Subviews.FirstOrDefault(sv => sv is UITextField) as UITextField).TextColor =
                inputColor;
            (subviews.FirstOrDefault().Subviews.FirstOrDefault(sv => sv is UITextField) as UITextField)
                .AttributedPlaceholder = new NSAttributedString(textCell.Placeholder, null,
                    inputColor.ColorWithAlpha(0.5f));

            if (textCell.IsPassword)
            {
                (subviews.FirstOrDefault().Subviews.FirstOrDefault(sv => sv is UITextField) as UITextField)
                    .SecureTextEntry = true;
            }


            return cell;
        }
    }
}