using CoreAnimation;
using CoreGraphics;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(LoadingButton), typeof(LoadingButtonRenderer))]

namespace MileEyes.iOS.Renderers
{
    public class LoadingButtonRenderer : ButtonRenderer
    {
        private CAShapeLayer circleLayer;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var elementHeight = 0.00;

            elementHeight = Element.MinimumHeightRequest > 1 ? Element.MinimumHeightRequest : 125.00;

            // Adjust frame size to make it perfectly square
            Control.Layer.Frame = new CGRect(0, 0, elementHeight, elementHeight);
            Control.Superview.Bounds = Control.Layer.Frame;
            
            // Make longer labels break on to multiple lines
            Control.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            Control.TitleLabel.TextAlignment = UITextAlignment.Center;

            // Provide some padding
            Control.TitleEdgeInsets = new UIEdgeInsets(5, 5, 5, 5);

            // Replace title
            Control.TitleLabel.RemoveFromSuperview();
            var loadingIndicator = new UIActivityIndicatorView();
            loadingIndicator.StartAnimating();
            loadingIndicator.Center = Control.Center;

            Control.AddSubview(loadingIndicator);


            // Create circle background color
            var textColor = Element.TextColor.ToUIColor();
            var backgroundColor = textColor.ColorWithAlpha(0.5f);


            if (circleLayer != null) return;

            // Create inner circle
            circleLayer = new CAShapeLayer
            {
                Path =
                    UIBezierPath.FromOval(new CGRect(3, 3, Control.Layer.Frame.Width - 6,
                            Control.Layer.Frame.Height - 6))
                        .CGPath,
                FillColor = backgroundColor.CGColor,
                Position = new CGPoint(0, 0)
            };

            Control.Layer.InsertSublayer(circleLayer, 0);


            // Creater static outer circle
            var staticCircleLayer = createOuterCircle(Control.Layer.Frame, backgroundColor);

            staticCircleLayer.Opacity = 1;

            Control.Layer.InsertSublayer(staticCircleLayer, 0);
        }

        private static CAShapeLayer createOuterCircle(CGRect circleSize, UIColor color)
        {
            var circleConstructor = new CAShapeLayer
            {
                Path = UIBezierPath.FromOval(circleSize).CGPath,
                Frame = circleSize
            };
            
            var circleAnchorPoint = new CGPoint(circleConstructor.Frame.GetMidX() / circleConstructor.Frame.GetMaxX(),
                circleConstructor.Frame.GetMidY() / circleConstructor.Frame.GetMaxY());

            circleConstructor.AnchorPoint = circleAnchorPoint;

            circleConstructor.StrokeColor = color.CGColor;

            circleConstructor.LineWidth = 2;

            circleConstructor.FillColor = UIColor.Clear.CGColor;

            circleConstructor.Opacity = 0;

            circleConstructor.ContentsGravity = CALayer.GravityCenter;

            return circleConstructor;
        }
    }
}