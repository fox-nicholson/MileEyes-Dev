using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public class BlurStackLayout : Frame
    {
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
            propertyName: "CornerRadius",
            returnType: typeof(float),
            declaringType: typeof(BlurStackLayout),
            defaultValue: 0f,
            defaultBindingMode: BindingMode.TwoWay);

        public float CornerRadius
        {
            get { return (float)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
    }
}
