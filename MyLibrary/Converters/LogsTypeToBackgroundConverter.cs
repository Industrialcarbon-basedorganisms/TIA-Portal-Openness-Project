using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyLibrary.Converters
{
    public class LogsTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString())
                {
                    case "Verbose":
                        return "#CCCCCC";
                    case "Debug":
                        return "#6A5ACD";
                    case "Information":
                        return "#2E8B57";
                    case "Warning":
                        return "#FFA500";
                    case "Error":
                        return "#B22222";
                    case "Fatal":
                        return "#8A2BE2";
                    default:
                        return "Transparent";
                }

            }
            return "Transparent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
