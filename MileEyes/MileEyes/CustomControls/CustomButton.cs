using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public class CustomButton : Button
    {
        public static readonly BindableProperty IsRedProperty = BindableProperty.Create(
            propertyName: "IsRed",
            returnType: typeof(bool),
            declaringType: typeof(CustomButton),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

        public bool IsRed
        {
            get { return (bool)GetValue(IsRedProperty); }
            set { SetValue(IsRedProperty, value); }
        }

        public static readonly BindableProperty PulseProperty = BindableProperty.Create(
            propertyName: "Pulse",
            returnType: typeof(bool),
            declaringType: typeof(CustomButton),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

        public bool Pulse
        {
            get { return (bool)GetValue(PulseProperty); }
            set { SetValue(PulseProperty, value); }
        }

        public event EventHandler FireAnimation = delegate { };

        public CustomButton()
        {
            Device.StartTimer(TimeSpan.FromSeconds(3), FireAnim);
        }

        public bool FireAnim()
        {
            if (!Pulse) return true;

            if (!Services.Host.Backgrounded)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    FireAnimation?.Invoke(this, EventArgs.Empty);
                });
            }
            return true;
        }
    }
}
