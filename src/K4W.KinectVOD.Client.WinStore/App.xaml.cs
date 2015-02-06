using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using K4W.KinectVOD.Client.Shared.Extensions;
using K4W.KinectVOD.Client.WinStore.LocalStorage;
using K4W.KinectVOD.Client.WinStore.Notifications;
using K4W.KinectVOD.Client.WinStore.Pages;
using K4W.KinectVOD.Shared;

using Microsoft.WindowsAzure.Messaging;

namespace K4W.KinectVOD.Client.WinStore
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Collection of all saved recordings
        /// </summary>
        private ObservableCollection<RecordingData> _recordingHistory = null;

        /// <summary>
        /// Name of the local file containing the recordings
        /// </summary>
        public const string RecordingFileName = "recording_history.json";

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // Switching decision between pages
            if (string.IsNullOrEmpty(e.Arguments))
            {
                // Register for notifications
                await RegisterPushNotifications("kinect-VOD-tutorial",
                                                "Endpoint=sb://kinect-demo.servicebus.windows.net/;SharedAccessKeyName=ListenPolicy;SharedAccessKey=KWaGKpa38A7ymRLDk/7qpTpZmla3nfmWN1AWto2R3zY=",
                                                "new-video-template",
                                                string.Format("$({0})", "RecordingData"),
                                                "New recorded video",
                                                string.Format("$({0})", "Caption"),
                                                "http://www.kinectingforwindows.com/images/notification_logo.png");

                // Navigate to the overview page
                rootFrame.Navigate(typeof(MainPage));
            }
            else
            {
                // Deserialize to RD
                RecordingData data = e.Arguments.DeserializeFromJson<RecordingData>();

                // Load recording history on first run
                if (_recordingHistory == null)
                    _recordingHistory = await LocalStorageHelper.LoadFileContentAsync<ObservableCollection<RecordingData>>(RecordingFileName) ??
                                        new ObservableCollection<RecordingData>();
                // Add to the list
                _recordingHistory.Add(data);

                // Save the new list locally
                await LocalStorageHelper.SaveFileContentAsync(RecordingFileName, _recordingHistory);

                // Navigate to the video page
                rootFrame.Navigate(typeof(VideoPage), data);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Register for push notifications or update the expiration when required
        /// </summary>
        /// <param name="hubName">Name of the sending hub</param>
        /// <param name="connectionString">Connection string to the Service Bus namespace</param>
        /// <param name="templateName">Name of the template</param>
        /// <param name="metadata">Notification property holding the metadata</param>
        /// <param name="header">Header text of the toast</param>
        /// <param name="footer">Footer text of the toast</param>
        /// <param name="image">Url to the image</param>
        public async Task RegisterPushNotifications(string hubName, string connectionString, string templateName, string metadata, string header, string footer, string image)
        {
            bool registerTemplate = false;

            // Retrieve local settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // Retrieve saved expiration date for this template
            object registerExpiration = localSettings.Values[templateName.Replace(" ", "-")];

            // Flag as to-register when no value found
            if (registerExpiration != null)
            {
                // Try parse to datetime
                DateTime expirationDateTime;
                DateTime.TryParse(registerExpiration.ToString(), out expirationDateTime);

                // Register when expired
                if (expirationDateTime <= DateTime.Now)
                    registerTemplate = true;
            }
            else
                registerTemplate = true;

            // Create a new registration when required
            if (registerTemplate == true)
            {
                TemplateRegistration tempRegistration = await PushNotificationsHelper.RegisterTemplateNotificationAsync(hubName, connectionString, templateName, metadata, header, footer, image);

                // Save new expiration date
                localSettings.Values[templateName.Replace(" ", "-")] = tempRegistration.ExpiresAt.ToString();
            }
        }

        #region Generated code
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        #endregion Generated code

    }
}
