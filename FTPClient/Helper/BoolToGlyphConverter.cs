using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FTPClient.Helper;

public class BoolToGlyphConverter : IValueConverter
{
    public object Convert(object value, Type _, object __, CultureInfo ___) =>
        (value is bool b && b) ? "▼" : "▶";

    public object ConvertBack(object value, Type _, object __, CultureInfo ___) =>
        throw new NotSupportedException();
}