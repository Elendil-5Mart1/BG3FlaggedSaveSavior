using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using BG3SaveSavior.Models;
using LSLib.LS;
using LSLib.LS.Enums;

namespace BG3SaveSavior.Services;

/// <summary>
/// Lit et répare le flag "Modded" des sauvegardes BG3 (meta.lsf dans le .lsv),
/// le flag qui conditionne le déblocage des succès Steam/GOG.
/// Utilise LSLib directement (même bibliothèque que Divine.exe / BG3 Mod Manager),
/// sans passer par un exécutable externe.
/// </summary>
public static class SaveScanService
{
    private const string MetaFileName = "meta.lsf";
    private const string SaveInfoFileName = "SaveInfo.json";

    public static IEnumerable<string> FindDefaultStoryFolders()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var profilesRoot = Path.Combine(localAppData, "Larian Studios", "Baldur's Gate 3", "PlayerProfiles");
        if (!Directory.Exists(profilesRoot))
            yield break;

        foreach (var profileDir in Directory.EnumerateDirectories(profilesRoot))
        {
            var story = Path.Combine(profileDir, "Savegames", "Story");
            if (Directory.Exists(story))
                yield return story;
        }
    }

    public static List<SaveEntry> ScanFolder(string storyRoot)
    {
        var results = new List<SaveEntry>();
        if (!Directory.Exists(storyRoot))
            return results;

        foreach (var campaignDir in Directory.EnumerateDirectories(storyRoot))
        {
            var campaignFolderName = Path.GetFileName(campaignDir);
            foreach (var lsv in Directory.EnumerateFiles(campaignDir, "*.lsv"))
            {
                results.Add(ScanSave(lsv, campaignFolderName));
            }
        }

        return results.OrderBy(e => e.CharacterName).ThenByDescending(e => e.LastWriteTime).ToList();
    }

    public static SaveEntry ScanSave(string lsvPath, string campaignFolderName)
    {
        var (characterName, _) = SplitCampaignFolderName(campaignFolderName);

        var entry = new SaveEntry
        {
            LsvPath = lsvPath,
            CampaignFolderName = campaignFolderName,
            CharacterName = characterName,
            LastWriteTime = File.GetLastWriteTime(lsvPath),
            DisplayName = Path.GetFileNameWithoutExtension(lsvPath),
        };

        try
        {
            var reader = new PackageReader();
            using var package = reader.Read(lsvPath);

            var saveInfoFile = package.Files.Find(f =>
                string.Equals(f.Name, SaveInfoFileName, StringComparison.OrdinalIgnoreCase));
            if (saveInfoFile != null)
            {
                using var s = saveInfoFile.CreateContentReader();
                using var doc = JsonDocument.Parse(s);
                if (doc.RootElement.TryGetProperty("Save Name", out var nameEl) && nameEl.GetString() is { Length: > 0 } saveName)
                {
                    entry.DisplayName = saveName;
                }
            }

            var metaFile = package.Files.Find(f =>
                string.Equals(f.Name, MetaFileName, StringComparison.OrdinalIgnoreCase));
            if (metaFile == null)
            {
                entry.Status = ModStatus.Error;
                entry.StatusMessage = "meta.lsf not found in this .lsv";
                return entry;
            }

            using var metaStream = metaFile.CreateContentReader();
            using var lsfReader = new LSFReader(metaStream, false);
            var resource = lsfReader.Read();

            var metaNode = FindMetaDataNode(resource);
            var moddedAttr = metaNode?.Attributes.GetValueOrDefault("Modded");
            if (moddedAttr == null)
            {
                entry.Status = ModStatus.Error;
                entry.StatusMessage = "Modded attribute not found in meta.lsf";
                return entry;
            }

            entry.Status = (bool)moddedAttr.Value! ? ModStatus.Flagged : ModStatus.Clean;

            // SaveGameType 0 = save manuelle (le jeu affiche "Save Name" tel quel) ;
            // 1 = Quicksave, 2 = Autosave (le jeu recalcule toujours son propre libellé
            // "<Type> <SaveGameID> - <Région>" à partir de meta.lsf, en ignorant
            // "Save Name" — renommer ces saves n'a donc aucun effet visible en jeu,
            // constaté le 2026-09-21 sur une Quicksave renommée.
            if (metaNode?.Attributes.GetValueOrDefault("SaveGameType")?.Value is byte saveGameType)
                entry.SaveGameType = saveGameType;
        }
        catch (Exception ex)
        {
            entry.Status = ModStatus.Error;
            entry.StatusMessage = ex.Message;
        }

        return entry;
    }

    /// <summary>
    /// Répare une sauvegarde flaguée : sauvegarde le .lsv original, extrait le package,
    /// repasse Modded à False dans meta.lsf, puis repackage et remplace le fichier original.
    /// Reproduit exactement la manipulation validée manuellement (Divine.exe) le 2026-09-20 :
    /// jeu cible BG3 (PackageVersion V18, LSF VerBG3Patch3, compression Zlib), avec le hash
    /// d'archive (MD5) activé pour éviter l'avertissement "tampering or corruption" au
    /// chargement — absent par défaut de LSLib/Divine.exe, cause identifiée le 2026-09-20.
    /// </summary>
    public static string Repair(SaveEntry entry, string backupRoot)
    {
        var (backupPath, workDir) = BackupAndUnpack(entry, backupRoot);
        try
        {
            var metaPath = Path.Combine(workDir, MetaFileName);
            if (!File.Exists(metaPath))
                throw new InvalidOperationException("meta.lsf not found after extraction.");

            var loadParams = ResourceLoadParameters.FromGameVersion(Game.BaldursGate3);
            loadParams.ByteSwapGuids = true;
            var resource = ResourceUtils.LoadResource(metaPath, loadParams);

            var moddedAttr = FindMetaDataNode(resource)?.Attributes.GetValueOrDefault("Modded")
                ?? throw new InvalidOperationException("Modded attribute not found in meta.lsf.");
            moddedAttr.Value = false;

            var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
            ResourceUtils.SaveResource(resource, metaPath, ResourceFormat.LSF, conversionParams);

            RepackAndReplace(workDir, entry.LsvPath);

            entry.Status = ModStatus.Clean;
            entry.StatusMessage = "";
            return backupPath;
        }
        finally
        {
            TryDelete(workDir);
        }
    }

    /// <summary>
    /// Renomme une sauvegarde : met à jour "Save Name" dans SaveInfo.json (le nom affiché
    /// dans le menu de sauvegarde du jeu, le vanilla BG3 n'offrant lui-même aucun moyen de
    /// renommer une save existante), puis renomme le(s) fichier(s) (.lsv, .WebP...) et le
    /// dossier contenant la sauvegarde pour rester cohérent avec la convention de nommage
    /// du jeu (CampaignKey__NomAffiché) et faciliter le repérage dans l'explorateur Windows.
    /// </summary>
    public static string RenameSave(SaveEntry entry, string newName, string backupRoot)
    {
        newName = newName.Trim();
        if (newName.Length == 0)
            throw new ArgumentException("The new name cannot be empty.", nameof(newName));

        var invalidChars = Path.GetInvalidFileNameChars();
        if (newName.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException("The new name contains characters that are not allowed in a file name.", nameof(newName));

        var (backupPath, workDir) = BackupAndUnpack(entry, backupRoot);
        try
        {
            var saveInfoPath = Path.Combine(workDir, SaveInfoFileName);
            if (File.Exists(saveInfoPath))
            {
                var node = JsonNode.Parse(File.ReadAllText(saveInfoPath))
                    ?? throw new InvalidOperationException("SaveInfo.json could not be parsed.");
                node["Save Name"] = newName;
                File.WriteAllText(saveInfoPath, node.ToJsonString());
            }

            RepackAndReplace(workDir, entry.LsvPath);
        }
        finally
        {
            TryDelete(workDir);
        }

        var folder = Path.GetDirectoryName(entry.LsvPath)!;
        var oldBaseName = Path.GetFileNameWithoutExtension(entry.LsvPath);
        foreach (var file in Directory.EnumerateFiles(folder))
        {
            if (!string.Equals(Path.GetFileNameWithoutExtension(file), oldBaseName, StringComparison.OrdinalIgnoreCase))
                continue;
            var newFile = Path.Combine(folder, newName + Path.GetExtension(file));
            if (!string.Equals(file, newFile, StringComparison.OrdinalIgnoreCase))
                File.Move(file, newFile);
        }

        var campaignKey = entry.CampaignFolderName.Split("__", 2)[0];
        var newFolder = Path.Combine(Path.GetDirectoryName(folder)!, $"{campaignKey}__{newName}");
        if (!string.Equals(folder, newFolder, StringComparison.OrdinalIgnoreCase))
            Directory.Move(folder, newFolder);

        entry.DisplayName = newName;
        return backupPath;
    }

    /// <summary>Copie de sauvegarde du .lsv puis extraction dans un dossier temporaire jetable.</summary>
    private static (string BackupPath, string WorkDir) BackupAndUnpack(SaveEntry entry, string backupRoot)
    {
        Directory.CreateDirectory(backupRoot);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(backupRoot, $"{entry.CharacterName}_{Path.GetFileNameWithoutExtension(entry.LsvPath)}_{timestamp}.lsv");
        File.Copy(entry.LsvPath, backupPath, overwrite: false);

        var workDir = Path.Combine(Path.GetTempPath(), "BG3SaveSavior", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        var reader = new PackageReader();
        using (var package = reader.Read(entry.LsvPath))
        {
            new Packager().UncompressPackage(package, workDir);
        }

        return (backupPath, workDir);
    }

    /// <summary>Repackage le contenu édité de workDir et remplace le .lsv original.</summary>
    private static void RepackAndReplace(string workDir, string lsvPath)
    {
        var tempLsv = workDir + ".lsv";
        try
        {
            var build = new PackageBuildData
            {
                Version = PackageVersion.V18,
                Compression = CompressionMethod.Zlib,
                CompressionLevel = LSCompressionLevel.Default,
                // Sans ça, l'entête du .lsv contient un MD5 à zéro (comportement par défaut
                // de LSLib/Divine.exe) et le jeu affiche "tampering or corruption" au chargement.
                Hash = true,
            };
            new Packager().CreatePackage(tempLsv, workDir, build).GetAwaiter().GetResult();
            File.Copy(tempLsv, lsvPath, overwrite: true);
        }
        finally
        {
            TryDelete(tempLsv);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
            else if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // best-effort cleanup du dossier temporaire, sans impact sur le résultat
        }
    }

    /// <summary>
    /// Structure confirmée par inspection réelle d'un meta.lsf BG3 :
    /// Regions["MetaData"] -> Children["MetaData"][0] -> Attributes["Modded"/"SaveGameType"/...].
    /// </summary>
    private static Node? FindMetaDataNode(Resource resource)
    {
        if (!resource.Regions.TryGetValue("MetaData", out var region))
            return null;
        if (!region.Children.TryGetValue("MetaData", out var metaNodes) || metaNodes.Count == 0)
            return null;

        return metaNodes[0];
    }

    private static (string CharacterName, string CampaignKey) SplitCampaignFolderName(string folderName)
    {
        var campaignKey = folderName.Split("__", 2)[0];
        var characterName = campaignKey.Split('-', 2)[0];
        return (string.IsNullOrWhiteSpace(characterName) ? folderName : characterName, campaignKey);
    }
}
