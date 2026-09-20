using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BG3SaveSavior.Interop;
using BG3SaveSavior.Models;
using BG3SaveSavior.Services;
using Microsoft.Win32;

namespace BG3SaveSavior;

public partial class MainWindow : Window
{
    private string? _storyRoot;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => WindowCorners.Apply(this);
        Loaded += (_, _) => AutoDetectAndScan();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void AutoDetectAndScan()
    {
        var detected = SaveScanService.FindDefaultStoryFolders().ToList();
        if (detected.Count == 0)
        {
            StatusBar.Text = "No save folder detected automatically. Use \"Browse\".";
            return;
        }

        _storyRoot = detected.FirstOrDefault(p => p.Contains($"{Path.DirectorySeparatorChar}Public{Path.DirectorySeparatorChar}"))
                     ?? detected[0];
        FolderPathText.Text = _storyRoot;
        Rescan();
    }

    private void Rescan()
    {
        if (_storyRoot == null) return;

        StatusBar.Text = "Scanning saves...";

        List<SaveEntry> results;
        try
        {
            results = SaveScanService.ScanFolder(_storyRoot);
        }
        catch (Exception ex)
        {
            StatusBar.Text = $"Scan failed: {ex.Message}";
            return;
        }

        var groups = results
            .GroupBy(e => e.CharacterName)
            .OrderBy(g => g.Key)
            .Select(g => new CampaignGroup
            {
                CampaignName = g.Key,
                Saves = g.OrderByDescending(e => e.LastWriteTime).ToList(),
            })
            .ToList();

        CampaignsControl.ItemsSource = groups;

        var flagged = results.Count(e => e.Status == ModStatus.Flagged);
        var clean = results.Count(e => e.Status == ModStatus.Clean);
        StatusBar.Text = $"{results.Count} save(s) in {groups.Count} campaign(s) — {clean} unlockable, {flagged} flagged.";
    }

    private void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose the BG3 save folder (…\\Savegames\\Story)",
            InitialDirectory = _storyRoot,
        };

        if (dialog.ShowDialog() == true)
        {
            _storyRoot = dialog.FolderName;
            FolderPathText.Text = _storyRoot;
            Rescan();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => Rescan();

    private async void RepairButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SaveEntry entry } button) return;
        if (_storyRoot == null) return;

        var confirm = MessageBox.Show(
            $"Repair the save \"{entry.DisplayName}\" ({entry.CharacterName})?\n\n" +
            "A backup of the original will be created before any change.\n" +
            "A \"corrupted file\" warning may appear the next time it loads in-game: " +
            "this is a harmless cosmetic side effect, already confirmed to have no impact on achievements.",
            "Confirm repair", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        button.IsEnabled = false;
        StatusBar.Text = $"Repairing \"{entry.DisplayName}\"...";

        try
        {
            var backupPath = await Task.Run(() => SaveScanService.Repair(entry, BackupRootFor(entry)));
            StatusBar.Text = $"\"{entry.DisplayName}\" repaired. Original backed up to: {backupPath}";
            Rescan();
        }
        catch (Exception ex)
        {
            button.IsEnabled = true;
            StatusBar.Text = $"Failed to repair \"{entry.DisplayName}\": {ex.Message}";
            MessageBox.Show($"Repair failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SaveEntry entry } button) return;
        if (_storyRoot == null) return;

        var dialog = new RenameDialog(entry.DisplayName) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        var newName = dialog.NewName;
        button.IsEnabled = false;
        StatusBar.Text = $"Renaming \"{entry.DisplayName}\" to \"{newName}\"...";

        try
        {
            var backupPath = await Task.Run(() => SaveScanService.RenameSave(entry, newName, BackupRootFor(entry)));
            StatusBar.Text = $"Renamed to \"{newName}\". Original backed up to: {backupPath}";
            Rescan();
        }
        catch (Exception ex)
        {
            button.IsEnabled = true;
            StatusBar.Text = $"Failed to rename \"{entry.DisplayName}\": {ex.Message}";
            MessageBox.Show($"Rename failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string BackupRootFor(SaveEntry entry) => Path.Combine(
        Path.GetDirectoryName(_storyRoot!.TrimEnd(Path.DirectorySeparatorChar))!,
        "Story_BG3SaveSavior_Backups",
        entry.CampaignFolderName);
}
