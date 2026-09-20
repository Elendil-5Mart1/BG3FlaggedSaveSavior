using System.Windows;
using System.Windows.Input;
using BG3SaveSavior.Interop;

namespace BG3SaveSavior;

public partial class RenameDialog : Window
{
    public string NewName { get; private set; } = "";

    public RenameDialog(string currentName)
    {
        InitializeComponent();
        SourceInitialized += (_, _) => WindowCorners.Apply(this);
        NameTextBox.Text = currentName;
        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Accept();
        else if (e.Key == Key.Escape) DialogResult = false;
    }

    private void RenameButtonOk_Click(object sender, RoutedEventArgs e) => Accept();

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Accept()
    {
        var name = NameTextBox.Text.Trim();
        if (name.Length == 0) return;
        NewName = name;
        DialogResult = true;
    }
}
