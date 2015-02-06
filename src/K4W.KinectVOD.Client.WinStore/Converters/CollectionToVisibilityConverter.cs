using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace K4W.KinectVOD.Client.WinStore.Converters
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)parameter == "ShowOnItemsPresent")
            {
                return (value == null) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
                return (value == null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
