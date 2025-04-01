using System.Globalization;
using CameraDemoApp.ViewModels;

namespace CameraDemoApp.Converters
{
    public class BarcodeResultSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BarcodeResultItemViewModel[] barcodeResultItemViewModels)
            {
                return barcodeResultItemViewModels.Select(vm => vm.BarcodeResult).ToArray();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}