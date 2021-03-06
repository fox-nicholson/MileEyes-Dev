﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public class CustomSwitchCell : SwitchCell, ICustomViewCell
    {
        public static readonly BindableProperty ShowDisclosureProperty = BindableProperty.Create(
               propertyName: "ShowDisclosure",
               returnType: typeof(bool),
               declaringType: typeof(CustomImageCell),
               defaultValue: true,
               defaultBindingMode: BindingMode.TwoWay);

        public bool ShowDisclosure
        {
            get { return (bool)GetValue(ShowDisclosureProperty); }
            set { SetValue(ShowDisclosureProperty, value); }
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
               propertyName: "ImageSource",
               returnType: typeof(ImageSource),
               declaringType: typeof(CustomSwitchCell),
               defaultValue: null,
               defaultBindingMode: BindingMode.TwoWay);
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
               propertyName: "ImageSource",
               returnType: typeof(Color),
               declaringType: typeof(CustomSwitchCell),
               defaultValue: Color.Default,
               defaultBindingMode: BindingMode.TwoWay);
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
    }
}
