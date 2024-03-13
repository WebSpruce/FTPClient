using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FTPClient.Helper;

public class FilePathToFileNameConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string filePath)
        {
            return System.IO.Path.GetFileName(filePath);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}