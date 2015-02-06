using Windows.UI.Xaml.Controls;

using K4W.KinectVOD.Client.WinStore.Common;

namespace K4W.KinectVOD.Client.WinStore.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
    }
}
