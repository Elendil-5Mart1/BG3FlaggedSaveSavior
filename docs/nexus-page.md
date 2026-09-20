# Nexus Mods presentation text

Draft content for the Nexus mod page. Written in plain markdown-ish
sections — reformat into Nexus's rich text editor (headers/bold/lists map
directly) when creating the page. Fill in the bracketed placeholders
before publishing.

## Title

> Flagged Save Savior

## Summary (short, shown in search results/tile)

> Restores achievement eligibility on saves flagged as "modded" — repairs
> the save itself, no reinstall or lost progress required.

## Tags / category

Category: **Save Games** (or **Miscellaneous** if a dedicated "tools"
category exists). Suggested tags: `achievements`, `save editor`, `utility`.

## Description

### The problem

BG3 blocks Steam/GOG achievements as soon as a mod is active, via a
`Modded` flag stored in the save file itself. Once that flag is set on a
save, the common belief — including among authors of other save-editing
tools — is that achievements are blocked for that save **forever**, even
after removing your mods and re-enabling achievements in-game (BG3SE +
`EnableAchievements`).

That belief is only true if you never touch the save file. The `Modded`
flag can be rewritten directly inside the save package, and that's what
this tool does.

### What it does

- **Scan**: detects all your saves (grouped by campaign/character) and
  shows which ones are flagged as modded.
- **Repair**: clears the `Modded` flag on a flagged save with one click.
  The original file is automatically backed up first — nothing is
  overwritten without a safety copy.
- **Rename**: renames a save (both its in-game display name and the
  file/folder on disk) for manual saves. Not available on Quicksave/
  Autosave slots — the game always computes its own label for those and
  ignores the stored name, regardless of what any tool does.

Verified in real conditions: a one-shot achievement unlocked immediately
after repairing a flagged save, and an **incremental** achievement kept
progressing normally afterwards on that repaired save — confirming this
restores genuine, ongoing achievement eligibility, not a one-off trick.

### ⚠ Known cosmetic issue

After a repair or rename, BG3 may show a "file corrupted/tampered"
warning the next time that save is loaded. This is a harmless side effect
of how the save archive gets repackaged (a known, unresolved issue in the
underlying library LSLib — [Norbyte/lslib#292 on
GitHub](https://github.com/Norbyte/lslib/issues/292), where the exact same
warning reproduces from a plain extract-and-repack with **zero**
modifications). It has **no effect on gameplay, save integrity, or
achievements** — confirmed through testing including live achievement
progression on a repaired save.

### Installation

1. Download the zip from the Files tab.
2. Extract anywhere.
3. Run `BG3FlaggedSaveSavior.exe` — no installation, no extra runtime required.

### Requirements

Windows 10/11 (64-bit). No other dependencies — the build is self-contained.

### Credits

Built on [LSLib](https://github.com/Norbyte/lslib) by Norbyte (MIT
license), the same library used by `Divine.exe` and BG3 Mod Manager to
read/write Baldur's Gate 3's save formats.

### Disclaimer

Not affiliated with Larian Studios. Always keep a backup of your saves —
this tool makes one automatically before any change, but a second copy
never hurts.

## Changelog

**v0.1.0** — first public release. Scan, Repair, Rename.
