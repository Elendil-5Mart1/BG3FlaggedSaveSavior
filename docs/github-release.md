# GitHub presentation text

Ready-to-paste text for the GitHub side of the publication.

## Repo name

`BG3FlaggedSaveSavior` (no dashes, matches the assembly/exe name).

## Repo "About" tagline (short description field)

> Restore Steam/GOG achievement eligibility on Baldur's Gate 3 saves flagged as modded — no reinstall, no lost progress.

Suggested topics/tags: `baldurs-gate-3`, `bg3`, `modding`, `savegame`, `wpf`, `dotnet`, `csharp`

## v0.1.0 release notes

Paste this as the description when publishing the GitHub Release (attach `BG3FlaggedSaveSavior-v0.1.0-win-x64.zip` as the binary asset).

---

### BG3 Flagged Save Savior v0.1.0 — first public release

A small Windows tool that restores Steam/GOG achievement eligibility on
**Baldur's Gate 3** saves flagged as "modded" — a state generally believed
to be permanent, but isn't. BG3 Flagged Save Savior rewrites the `Modded`
flag directly inside the save file and repackages it, so achievements
unlock normally again as you keep playing that save.

**Features**

- Scan all your saves, grouped by campaign/character, with a clear
  clean/flagged indicator per save.
- One-click **Repair**: clears the `Modded` flag, with an automatic backup
  of the original save before any change.
- **Rename** saves (in-game name + file/folder on disk) for manual saves.

**Verified in real conditions**: the endgame achievement **Tactician**
unlocked after repairing a save that had been flagged as modded and was
taken right before the final boss fight, then finishing the fight on that
repaired save. The incremental achievement **Punch Drunk** also progressed
normally (0/20 → 1/20) after repairing a save and continuing to play it —
confirming this restores real, ongoing achievement eligibility across an
entire playthrough, not just a one-time trigger.

**Requirements**: this tool only fixes the save file — it doesn't
re-enable achievements in the game itself. You need a working achievement
enabler already installed. Tested with [BG3 Mod Manager (LaughingLeader)
v1.0.12.9](https://github.com/LaughingLeader/BG3ModManager/releases/tag/1.0.12.9)
(Achievement Enabler option checked) + Script Extender (BG3SE). Other
achievement enablers aren't tested.

**Known cosmetic issue**: the game may show a "file corrupted/tampered"
warning after loading a repaired/renamed save. This is a harmless
byproduct of repackaging the save archive (see
[Norbyte/lslib#292](https://github.com/Norbyte/lslib/issues/292) — same
symptom occurs from a plain extract+repack with zero modifications). No
effect on gameplay, save integrity, or achievements.

**Download**: grab `BG3FlaggedSaveSavior-v0.1.0-win-x64.zip` below, unzip,
run `BG3FlaggedSaveSavior.exe`. No installation, no .NET runtime required
(self-contained build).

See the [README](../README.md) for details on how it works.

---
