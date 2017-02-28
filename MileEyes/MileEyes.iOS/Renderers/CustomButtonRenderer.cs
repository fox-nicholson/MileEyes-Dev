using CoreAnimation;
using CoreGraphics;
using Foundation;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonRenderer))]

namespace MileEyes.iOS.Renderers
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        private CAShapeLayer outerCircleLayer;
        private CAShapeLayer rippleCircleLayer;
        private double elementHeight;
        private UIColor textColor;
        private UIColor backgroundColor;

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                elementHeight = Element.HeightRequest > 1 ? Element.HeightRequest : 125.00;

                textColor = Element.TextColor.ToUIColor();
                backgroundColor = textColor.ColorWithAlpha(0.5f);
            }

            var customButton = Element as CustomButton;

            if (customButton == null) return;

            customButton.FireAnimation += (sender, en) =>
            {
                if (Element == null) return;

                // Create ripple 1 circle
                outerCircleLayer = createOuterCircle(Control.Layer.Frame, backgroundColor);

                Control.Layer.InsertSublayer(outerCircleLayer, 0);


                // Create ripple 2 circle
                rippleCircleLayer = createOuterCircle(Control.Layer.Frame, backgroundColor);

                Control.Layer.InsertSublayer(rippleCircleLayer, 0);

                animation1(Element);
                animation2(Element);
            };
        }

        private CAShapeLayer circleLayer;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();


            // Adjust frame size to make it perfectly square
            Control.Layer.Frame = new CGRect(0, 0, elementHeight, elementHeight);
            Control.Superview.Bounds = Control.Layer.Frame;

            // Align Control in Superview
            //Control.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            //Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;


            // Make longer labels break on to multiple lines
            Control.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            Control.TitleLabel.TextAlignment = UITextAlignment.Center;

            // Provide some padding
            Control.TitleEdgeInsets = new UIEdgeInsets(5, 5, 5, 5);

            //Control.Layer.BackgroundColor = new CGColor(0.0f, 0.0f, 0.0f, 0.2f);

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

        public void animation1(Button button)
        {
            var scaleAnimation = CABasicAnimation.FromKeyPath("transform");

            scaleAnimation.From = NSValue.FromCATransform3D(CATransform3D.MakeScale(1, 1, 1));
            scaleAnimation.To = NSValue.FromCATransform3D(CATransform3D.MakeScale(2, 2, 1));


            var opacityAnimation = CABasicAnimation.FromKeyPath("opacity");

            opacityAnimation.From = NSNumber.FromFloat(0.15f);
            opacityAnimation.To = NSNumber.FromFloat(0.0f);


            var buttonAnimationGroup = new CAAnimationGroup
            {
                Duration = 2.0f,
                RemovedOnCompletion = true,
                FillMode = CAFillMode.Both,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut),
                Animations = new CAAnimation[] {scaleAnimation, opacityAnimation}
            };


            outerCircleLayer.AddAnimation(buttonAnimationGroup, "allMyAnimations");
        }

        public void animation2(Button button)
        {
            var scaleAnimation = CABasicAnimation.FromKeyPath("transform");

            scaleAnimation.From = NSValue.FromCATransform3D(CATransform3D.MakeScale(1, 1, 1));
            scaleAnimation.To = NSValue.FromCATransform3D(CATransform3D.MakeScale(2, 2, 1));


            var opacityAnimation = CABasicAnimation.FromKeyPath("opacity");

            opacityAnimation.From = NSNumber.FromFloat(0.25f);
            opacityAnimation.To = NSNumber.FromFloat(0.0f);

            var rippleAnimationGroup = new CAAnimationGroup
            {
                Duration = 2.0f,
                TimeOffset = -0.25f,
                RemovedOnCompletion = true,
                FillMode = CAFillMode.Both,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut),
                Animations = new CAAnimation[] {scaleAnimation, opacityAnimation}
            };


            rippleCircleLayer.AddAnimation(rippleAnimationGroup, "rippleMyAnimations");
        }
    }
}