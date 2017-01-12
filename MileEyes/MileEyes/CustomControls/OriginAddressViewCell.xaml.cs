using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public partial class OriginAddressViewCell : ViewCell, ICustomViewCell
    {
        public static readonly BindableProperty ShowDisclosureProperty = BindableProperty.Create(
            propertyName: "ShowDisclosure",
            returnType: typeof(bool),
            declaringType: typeof(CustomButton),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

        public bool ShowDisclosure
        {
            get { return (bool)GetValue(ShowDisclosureProperty); }
            set { SetValue(ShowDisclosureProperty, value); }
        }
        public OriginAddressViewCell()
        {
            InitializeComponent();
        }
    }
}
