using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Pages;
using Xamarin.Forms;

namespace MileEyes
{
    public partial class AndroidMainPage : TabbedPage
    {
        public AndroidMainPage()
        {
            InitializeComponent();

            JourneysPage.GotoTrackPage += JourneysPage_GotoTrackPage;
        }

        private void JourneysPage_GotoTrackPage(object sender, EventArgs e)
        {
            CurrentPage = Children[1];
        }
    }
}
