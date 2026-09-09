using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace lab1.UI.Converters;

public sealed class NullableIntToTextConverter : IValueConverter
{
    public string NullText { get; set; } = "-";

    public string Prefix { get; set; } = "#";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int number)
            return string.Concat(Prefix, number.ToString(culture));

        return NullText;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
