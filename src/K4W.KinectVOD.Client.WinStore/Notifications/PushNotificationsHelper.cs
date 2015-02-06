using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Microsoft.WindowsAzure.Messaging;

namespace K4W.KinectVOD.Client.WinStore.Notifications
{
    public class PushNotificationsHelper
    {
        /// <summary>
        /// Register a template notification
        /// </summary>
        /// <param name="hubName">Name of the sending hub</param>
        /// <param name="connectionString">Connection string to the Service Bus namespace</param>
        /// <param name="templateName">Name of the template</param>
        /// <param name="metadata">Notification property holding the metadata</param>
        /// <param name="header">Header text of the toast</param>
        /// <param name="footer">Footer text of the toast</param>
        /// <param name="image">Url to the image</param>
        /// <returns></returns>
        public static async Task<TemplateRegistration> RegisterTemplateNotificationAsync(string hubName, string connectionString, string templateName, string metadata, string header, string footer, string image)
        {
            // Create a new push notification channel
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            // Create a new notification hub
            NotificationHub hub = new NotificationHub(hubName, connectionString);

            // Generate the template for the toast
            XmlDocument toastTemplate = await GenerateXmlTemplateAsync(metadata, header, footer, image);

            // Register the template
            return await hub.RegisterTemplateAsync(channel.Uri, toastTemplate, templateName);
        }

        /// <summary>
        /// Generate the Xml Template for the 'ToastImageAndText02' notification
        /// </summary>
        /// <param name="metadata">Notification property holding the metadata</param>
        /// <param name="header">Header text of the toast</param>
        /// <param name="footer">Footer text of the toast</param>
        /// <param name="image">Url to the image</param>
        private static async Task<XmlDocument> GenerateXmlTemplateAsync(string metadata, string header, string footer, string image)
        {
            var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // msg, id, url, tag
            template.DocumentElement.SetAttribute("launch", metadata);

            var titleNode = template.SelectSingleNode("//text[@id='1']") as XmlElement;
            if (titleNode != null)
            {
                titleNode.InnerText = header;
            }

            var captionNode = template.SelectSingleNode("//text[@id='2']") as XmlElement;
            if (captionNode != null)
            {
                captionNode.InnerText = footer;
            }

            var imgNode = template.SelectSingleNode("//image[@id='1']") as XmlElement;
            if (imgNode != null)
            {
                imgNode.SetAttribute("src", image);
                imgNode.SetAttribute("alt", image);
            }

            return template;
        }
    }
}
