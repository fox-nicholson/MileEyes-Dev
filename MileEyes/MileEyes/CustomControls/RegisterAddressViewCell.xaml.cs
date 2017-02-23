using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public partial class RegisterAddressViewCell : ViewCell, ICustomViewCell
    {
        public RegisterAddressViewCell()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            propertyName: "ImageSource",
            returnType: typeof(ImageSource),
            declaringType: typeof(RegisterAddressViewCell),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: "ImageSource",
            returnType: typeof(Color),
            declaringType: typeof(RegisterAddressViewCell),
            defaultValue: Color.Default,
            defaultBindingMode: BindingMode.TwoWay);

        public Color TextColor
        {
            get { return (Color) GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public bool ShowDisclosure { get; set; }
    }
}