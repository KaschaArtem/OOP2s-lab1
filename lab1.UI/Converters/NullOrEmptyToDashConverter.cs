using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace lab1.UI.Converters;

public sealed class NullOrEmptyToDashConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? "-" : value!;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}