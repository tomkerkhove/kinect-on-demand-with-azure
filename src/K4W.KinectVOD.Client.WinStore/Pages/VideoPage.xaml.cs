using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using K4W.KinectVOD.Client.WinStore.Common;
using K4W.KinectVOD.Shared;

namespace K4W.KinectVOD.Client.WinStore.Pages
{
    public sealed partial class VideoPage : Page
    {
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public VideoPage()
        {
            this.InitializeComponent();
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            defaultViewModel["RecordingData"] = (e.Parameter) as RecordingData;
        }

        /// <summary>
        /// Return back to the global overview
        /// </summary>
        private void OnBackButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (MainPage));
        }
    }
}
