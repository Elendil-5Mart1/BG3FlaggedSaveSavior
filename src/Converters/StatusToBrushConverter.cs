using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BG3SaveSavior.Models;

namespace BG3SaveSavior.Converters;

/// <summary>Convertit un ModStatus en pastille de couleur (rouge = flaguée, vert = propre).</summary>
public class StatusToBrushConverter : IValueConverter
{
    private static readonly Brush Clean = new SolidColorBrush(Color.FromRgb(0x2E, 0xA0, 0x4A));
    private static readonly Brush Flagged = new SolidColorBrush(Color.FromRgb(0xD6, 0x33, 0x33));
    private static readonly Brush Error = new SolidColorBrush(Color.FromRgb(0xB0, 0x8A, 0x00));
    private static readonly Brush Unknown = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A));

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        ModStatus.Clean => Clean,
        ModStatus.Flagged => Flagged,
        ModStatus.Error => Error,
        _ => Unknown,
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
