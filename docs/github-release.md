# GitHub presentation text

Ready-to-paste text for the GitHub side of the publication.

## Repo "About" tagline (short description field)

> Restore Steam/GOG achievement eligibility on Baldur's Gate 3 saves flagged as modded — no reinstall, no lost progress.

Suggested topics/tags: `baldurs-gate-3`, `bg3`, `modding`, `savegame`, `wpf`, `dotnet`, `csharp`

## v0.1.0 release notes

Paste this as the description when publishing the GitHub Release (attach `BG3SaveSavior-v0.1.0-win-x64.zip` as the binary asset).

---

### BG3 Save Savior v0.1.0 — first public release

A small Windows tool that restores Steam/GOG achievement eligibility on
**Baldur's Gate 3** saves flagged as "modded" — a state generally believed
to be permanent, but isn't. BG3 Save Savior rewrites the `Modded` flag
directly inside the save file and repackages it, so achievements unlock
normally again as you keep playing that save.

**Features**

- Scan all your saves, grouped by campaign/character, with a clear
  clean/flagged indicator per save.
- One-click **Repair**: clears the `Modded` flag, with an automatic backup
  of the original save before any change.
- **Rename** saves (in-game name + file/folder on disk) for manual saves.

**Verified in real conditions**: a one-shot achievement unlocked right
after repairing a flagged save, and an incremental achievement progressed
normally afterwards on the repaired save — confirming this restores real,
ongoing achievement eligibility.

**Known cosmetic issue**: the game may show a "file corrupted/tampered"
warning after loading a repaired/renamed save. This is a harmless
byproduct of repackaging the save archive (see
[Norbyte/lslib#292](https://github.com/Norbyte/lslib/issues/292) — same
symptom occurs from a plain extract+repack with zero modifications). No
effect on gameplay, save integrity, or achievements.

**Download**: grab `BG3SaveSavior-v0.1.0-win-x64.zip` below, unzip, run
`BG3SaveSavior.exe`. No installation, no .NET runtime required
(self-contained build).

See the [README](../README.md) for details on how it works.

---
