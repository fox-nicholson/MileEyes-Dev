using System;
using System.Collections.Generic;
using System.Text;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MileEyesViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(DefaultCompanyViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(CompanyViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(CompanyItemViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(EngineTypeCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(EngineTypeItemViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(VehiclesViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(VehicleItemViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(VehicleViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(DefaultVehicleViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(ReasonsViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(ReasonItemViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(DefaultReasonViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(ReasonViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(DefaultPassengersViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(PassengersViewCell), typeof(CustomTextViewCellRenderer))]

[assembly: ExportRenderer(typeof(DestinationAddressViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(OriginAddressViewCell), typeof(CustomTextViewCellRenderer))]
[assembly: ExportRenderer(typeof(RegisterAddressViewCell), typeof(CustomTextViewCellRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomTextViewCellRenderer : ViewCellRenderer
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
