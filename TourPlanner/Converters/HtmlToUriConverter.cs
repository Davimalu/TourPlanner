using System;
using System.Globalization;
using System.Windows.Data;

namespace TourPlanner.Converters
{
    public class HtmlToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => new Uri("about:blank"); // Initiale Navigation für WebView2

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}