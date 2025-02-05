using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Utilities.AppLibrary;
using System;
using System.Globalization;

namespace Ryujinx.Ava.UI.Helpers
{
    internal class MultiplayerInfoConverter : MarkupExtension, IValueConverter
    {
        private static readonly MultiplayerInfoConverter _instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ApplicationData { HasLdnGames: true } applicationData)
            {
                return $"Hosted Games: {applicationData.GameCount}\nOnline Players: {applicationData.PlayerCount}";
            }
            
            return "";
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance;
        }
    }
}
