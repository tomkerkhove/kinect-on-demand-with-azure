using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Notifications;

namespace K4W.KinectVOD.Client.WPF.MicrosoftAzure
{
    public class NotificationHubAgent
    {
        /// <summary>
        /// Notification hub client to certain hub
        /// </summary>
        private NotificationHubClient _hubClient;

        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="hubName">Name of the requested notification hub</param>
        /// <param name="connectionString">Connection string to the Service Bus namespace</param>
        public NotificationHubAgent(string hubName, string connectionString)
        {
            if (string.IsNullOrEmpty(hubName)) throw new ArgumentException("Invalid hub name.");
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid Service Bus connection string.");

            // Create a new hub client
            _hubClient = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
        }

        /// <summary>
        /// Send a template notification (Platform independent)
        /// </summary>
        /// <param name="properties">Set of properties</param>
        public async Task SendTemplateNotificationAsync(Dictionary<string, string> properties)
        {
            if (properties == null) throw new ArgumentException("Properties cannot be Null.");

            // Send
            await _hubClient.SendTemplateNotificationAsync(properties);
        }
    }
}
