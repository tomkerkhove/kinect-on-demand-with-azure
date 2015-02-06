using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using K4W.KinectVOD.Client.WinStore.Common;
using K4W.KinectVOD.Client.WinStore.LocalStorage;
using K4W.KinectVOD.Client.WinStore.Pages;
using K4W.KinectVOD.Shared;

namespace K4W.KinectVOD.Client.WinStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.DefaultViewModel["Items"] = await LocalStorageHelper.LoadFileContentAsync<ObservableCollection<RecordingData>>(App.RecordingFileName) ?? new ObservableCollection<RecordingData>();
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(VideoPage), (e.ClickedItem) as RecordingData);
        }
    }
}
