using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public class CustomEntryCell : EntryCell
    {
        public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
            propertyName: "IsPassword",
            returnType: typeof(bool),
            declaringType: typeof(CustomButton),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }
    }
}
