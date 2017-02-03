using System.Collections.Generic;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(ContentPageRenderer))]

namespace MileEyes.iOS.Renderers
{
    internal class ContentPageRenderer : PageRenderer
    {
        private List<UIBarButtonItem> leftNavList;
        private List<UIBarButtonItem> rightNavList;
        private IList<ToolbarItem> toolbarItems;
        public new ContentPage Element => (ContentPage) base.Element;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController == null) return;

            var navigationItem = NavigationController.TopViewController.NavigationItem;

            if (leftNavList == null && rightNavList == null)
            {
                leftNavList = new List<UIBarButtonItem>();
                rightNavList = new List<UIBarButtonItem>();

                for (var i = 0; i < Element.ToolbarItems.Count; i++)
                {
                    var reorder = Element.ToolbarItems.Count - 1;
                    var itemPriority = Element.ToolbarItems[reorder - i].Priority;

                    var toolbatItem = Element.ToolbarItems[i];

                    switch (toolbatItem.Priority)
                    {
                        case 1:
                            var leftNavItems = navigationItem.RightBarButtonItems[i];
                            if (leftNavItems.Title == "Cancel" || leftNavItems.Title == "Delete")
                                leftNavItems.TintColor = UIColor.Red;
                            leftNavList.Add(leftNavItems);
                            break;
                        case 0:
                            var rightNavItems = navigationItem.RightBarButtonItems[i];
                            switch (rightNavItems.Title)
                            {
                                case "Share":
                                {
                                    var newItem = new UIBarButtonItem(UIBarButtonSystemItem.Action)
                                    {
                                        Action = rightNavItems.Action,
                                        Target = rightNavItems.Target,
                                        TintColor = UIColor.Green
                                    };
                                    rightNavList.Add(newItem);
                                }
                                    break;
                                case "Save":
                                {
                                    var newItem = new UIBarButtonItem(UIBarButtonSystemItem.Save)
                                    {
                                        Action = rightNavItems.Action,
                                        Target = rightNavItems.Target,
                                        TintColor = UIColor.Green
                                    };
                                    rightNavList.Add(newItem);
                                }
                                    break;
                                case "Done":
                                    rightNavItems.TintColor = UIColor.Green;
                                    rightNavList.Add(rightNavItems);
                                    break;
                                case "Add":
                                {
                                    var newItem = new UIBarButtonItem(UIBarButtonSystemItem.Add)
                                    {
                                        Action = rightNavItems.Action,
                                        Target = rightNavItems.Target,
                                        TintColor = UIColor.Green
                                    };
                                    rightNavList.Add(newItem);
                                }
                                    break;
                                default:
                                    rightNavList.Add(rightNavItems);
                                    break;
                            }
                            break;
                    }
                }
            }

            navigationItem.SetLeftBarButtonItems(leftNavList.ToArray(), false);
            navigationItem.SetRightBarButtonItems(rightNavList.ToArray(), false);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }
    }
}