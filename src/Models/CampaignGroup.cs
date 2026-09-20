namespace BG3SaveSavior.Models;

/// <summary>Regroupe les sauvegardes d'une même campagne/personnage, façon menu de sauvegarde du jeu.</summary>
public class CampaignGroup
{
    public required string CampaignName { get; init; }
    public required List<SaveEntry> Saves { get; init; }

    public ModStatus AggregateStatus =>
        Saves.Any(s => s.Status == ModStatus.Flagged) ? ModStatus.Flagged :
        Saves.Any(s => s.Status == ModStatus.Error) ? ModStatus.Error :
        Saves.All(s => s.Status == ModStatus.Clean) ? ModStatus.Clean :
        ModStatus.Unknown;

    /// <summary>Déplié par défaut uniquement si une action est possible dedans (comme le menu du jeu, mais utile).</summary>
    public bool NeedsAttention => AggregateStatus == ModStatus.Flagged;
}
