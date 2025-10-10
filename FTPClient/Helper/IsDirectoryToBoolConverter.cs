using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FTPClient.Models;

namespace FTPClient.Helper;

public class IsDirectoryToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Directory;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}