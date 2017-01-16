using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public partial class RegisterAddressViewCell : ViewCell, ICustomViewCell
    {
        public RegisterAddressViewCell()
        {
            InitializeComponent();
        }

        public bool ShowDisclosure { get; set; }
    }
}
