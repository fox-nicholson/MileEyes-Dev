using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public partial class PassengersViewCell : ViewCell, ICustomViewCell
    {
        public static readonly BindableProperty ShowDisclosureProperty = BindableProperty.Create(
            propertyName: "ShowDisclosure",
            returnType: typeof(bool),
            declaringType: typeof(CustomButton),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

        public bool ShowDisclosure
        {
            get { return (bool) GetValue(ShowDisclosureProperty); }
            set { SetValue(ShowDisclosureProperty, value); }
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            propertyName: "ImageSource",
            returnType: typeof(ImageSource),
            declaringType: typeof(PassengersViewCell),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: "ImageSource",
            returnType: typeof(Color),
            declaringType: typeof(PassengersViewCell),
            defaultValue: Color.Default,
            defaultBindingMode: BindingMode.TwoWay);

        public Color TextColor
        {
            get { return (Color) GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public PassengersViewCell()
        {
            InitializeComponent();
        }
    }
}