using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BG3SaveSavior.Models;

public enum ModStatus
{
    Unknown,
    Clean,
    Flagged,
    Error,
}

public class SaveEntry : INotifyPropertyChanged
{
    public required string LsvPath { get; init; }
    public required string CampaignFolderName { get; init; }
    public required string CharacterName { get; init; }
    public required DateTime LastWriteTime { get; init; }

    public string DisplayName { get; set; } = "";

    private ModStatus _status = ModStatus.Unknown;
    public ModStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(CanRepair));
        }
    }

    public string StatusMessage { get; set; } = "";

    public string StatusLabel => Status switch
    {
        ModStatus.Clean => "Achievements unlockable",
        ModStatus.Flagged => "Achievements blocked",
        ModStatus.Error => "Read error",
        _ => "Scanning...",
    };

    public bool CanRepair => Status == ModStatus.Flagged;

    /// <summary>
    /// 0 = save manuelle (le jeu affiche "Save Name" tel quel), 1 = Quicksave,
    /// 2 = Autosave (le jeu recalcule toujours son propre libellé à partir de
    /// meta.lsf, en ignorant "Save Name" — renommer n'a donc aucun effet visible).
    /// Null si non lu (erreur de scan avant d'atteindre cet attribut).
    /// </summary>
    public byte? SaveGameType { get; set; }

    public bool CanRename => SaveGameType == 0;

    public string FileName => Path.GetFileName(LsvPath);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
