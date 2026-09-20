using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BG3SaveSavior.Interop;

/// <summary>Coins arrondis natifs Windows 11 pour les fenêtres sans bordure, partagé entre MainWindow et RenameDialog.</summary>
public static class WindowCorners
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwcpRound = 2;

    public static void Apply(Window window)
    {
        try
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var preference = DwmwcpRound;
            DwmSetWindowAttribute(hwnd, DwmwaWindowCornerPreference, ref preference, sizeof(int));
        }
        catch
        {
            // Windows 10 ou plus ancien : coins carrés, sans conséquence.
        }
    }
}
