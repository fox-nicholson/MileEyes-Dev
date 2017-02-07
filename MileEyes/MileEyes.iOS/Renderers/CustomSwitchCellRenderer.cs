using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomSwitchCell), typeof(CustomSwitchCellRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomSwitchCellRenderer : SwitchCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var customViewCell = item as CustomSwitchCell;

            var cell = (CellTableViewCell)base.GetCell(item, reusableCell, tv);

            cell.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 25);

            (cell.Subviews.FirstOrDefault().Subviews.FirstOrDefault() as UILabel).TextColor = customViewCell.TextColor.ToUIColor();

            if (customViewCell.ShowDisclosure)
            {
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }

            cell.SeparatorInset = new UIEdgeInsets(0, 50, 0, 0);

            SetImage(customViewCell, cell);

            return cell;
        }

        async void SetImage(CustomSwitchCell cell, CellTableViewCell target)
        {
            var source = cell.ImageSource;

            target.ImageView.Image = null;

            IImageSourceHandler handler = GetHandler(source);

            if (source != null && handler != null)
            {
                UIImage uiimage;
                try
                {
                    uiimage = await handler.LoadImageAsync(source).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    uiimage = null;
                }

                NSRunLoop.Main.BeginInvokeOnMainThread(() =>
                {
                    target.ImageView.Image = uiimage;
                    target.SetNeedsLayout();
                });
            }
            else
                target.ImageView.Image = null;
        }

        private static IImageSourceHandler GetHandler(ImageSource source)
        {
            IImageSourceHandler returnValue = null;
            if (source is UriImageSource)
            {
                returnValue = new ImageLoaderSourceHandler();
            }
            else if (source is FileImageSource)
            {
                returnValue = new FileImageSourceHandler();
            }
            else if (source is StreamImageSource)
            {
                returnValue = new StreamImagesourceHandler();
            }
            return returnValue;
        }
    }
}
