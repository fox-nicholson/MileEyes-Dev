using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MileEyes.CustomControls.CustomTableView), typeof(MileEyes.iOS.Renderers.CustomTableViewRenderer))]
namespace MileEyes.iOS.Renderers
{
    public class CustomTableViewRenderer : TableViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                return;

            var tableView = Control as UITableView;

            // tableView.BackgroundColor = new UIColor(0.03f, 0.05f, 0.06f, 1.0f);

            tableView.SeparatorColor = new UIColor(1.0f, 1.0f, 1.0f, 0.1f);
        }
    }
}
