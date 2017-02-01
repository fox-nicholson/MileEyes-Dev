using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using MileEyes.CustomControls;
using MileEyes.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SegmentedControl), typeof(SegmentedControlRenderer))]

namespace MileEyes.Droid.Renderers
{
    public class SegmentedControlRenderer : ViewRenderer<SegmentedControl, RadioGroup>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SegmentedControl> e)
        {
            base.OnElementChanged(e);

            var layoutInflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);

            var g = new RadioGroup(Context);
            g.Orientation = Orientation.Horizontal;

            for (var i = 0; i < e.NewElement.Children.Count; i++)
            {
                var o = e.NewElement.Children[i];
                var v = (SegmentedControlButton) layoutInflater.Inflate(Resource.Layout.SegmentedControl, null);
                v.Text = o.Text;
                if (i == 0)
                    v.SetBackgroundResource(Resource.Drawable.segmented_control_first_background);
                else if (i == e.NewElement.Children.Count - 1)
                    v.SetBackgroundResource(Resource.Drawable.segmented_control_last_background);
                g.AddView(v);
            }
            var child = g.GetChildAt(0);
            
            g.Check(child.Id);

            g.CheckedChange += (sender, eventArgs) =>
            {
                var rg = (RadioGroup) sender;
                if (rg.CheckedRadioButtonId != -1)
                {
                var id = rg.CheckedRadioButtonId;
                var radioButton = rg.FindViewById(id);
                var radioId = rg.IndexOfChild(radioButton);
                var btn = (SegmentedControlButton) rg.GetChildAt(radioId);
                var selection = btn.Text;
                e.NewElement.SelectedValue = selection;
                }
            };
            SetNativeControl(g);
        }
    }

    public class SegmentedControlButton : RadioButton
    {
        private int lineHeightSelected;
        private int lineHeightUnselected;

        private Paint linePaint;

        public SegmentedControlButton(Context context) : this(context, null)
        {
        }

        public SegmentedControlButton(Context context, IAttributeSet attributes)
            : this(context, attributes, Resource.Attribute.segmentedControlOptionStyle)
        {
        }

        public SegmentedControlButton(Context context, IAttributeSet attributes, int defStyle)
            : base(context, attributes, defStyle)
        {
            Initialize(attributes, defStyle);
        }

        private void Initialize(IAttributeSet attributes, int defStyle)
        {
            var a = Context.ObtainStyledAttributes(attributes, Resource.Styleable.SegmentedControlOption, defStyle,
                Resource.Style.SegmentedControlOption);

            var lineColor = a.GetColor(Resource.Styleable.SegmentedControlOption_lineColor, 0);
            linePaint = new Paint();
            linePaint.Color = lineColor;

            lineHeightUnselected =
                a.GetDimensionPixelSize(Resource.Styleable.SegmentedControlOption_lineHeightUnselected, 0);
            lineHeightSelected = a.GetDimensionPixelSize(Resource.Styleable.SegmentedControlOption_lineHeightSelected, 0);

            a.Recycle();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (linePaint.Color != 0 && (lineHeightSelected > 0 || lineHeightUnselected > 0))
            {
                var lineHeight = Checked ? lineHeightSelected : lineHeightUnselected;

                if (lineHeight > 0)
                {
                    var rect = new Rect(0, Height - lineHeight, Width, Height);
                    canvas.DrawRect(rect, linePaint);
                }
            }
        }
    }
}