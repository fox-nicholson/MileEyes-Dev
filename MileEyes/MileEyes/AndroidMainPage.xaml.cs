using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Pages;
using Xamarin.Forms;

namespace MileEyes
{
    /// <summary>
    /// MainPage for Android Variant
    /// </summary>
    public partial class AndroidMainPage : TabbedPage
    {
        public AndroidMainPage()
        {
            InitializeComponent();

            // Hook for switching to Track Journey Page in TabbedPage
            JourneysPage.GotoTrackPage += JourneysPage_GotoTrackPage;
        }

        private void JourneysPage_GotoTrackPage(object sender, EventArgs e)
        {
            // Switch to the second tab (Which is Track Journey)
            CurrentPage = Children[1];
        }
    }
}
