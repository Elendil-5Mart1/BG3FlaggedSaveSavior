# Nexus Mods presentation text

Ready-to-paste content, matching Nexus's own mod-page form fields exactly
(General tab + Full description's 5 built-in sections).

## General tab

- **Mod Name**: `Flagged Save Savior`
- **Game**: Baldur's Gate 3
- **Category**: Utilities
- **Mod version**: `0.1.0` (keep in sync with the GitHub release tag)
- **Author or team name**: (as filled on Nexus)
- **Short description** (250 chars max):
  > C# app that restores achievement eligibility on saves flagged as "modded" — repairs the save itself.

## Full description

Note: Nexus's rich text editor renders backtick/code spans as a full-width
block, not inline code — the text below intentionally avoids backticks
(plain text or quotes instead) so it pastes cleanly.

### Description

BG3 blocks Steam/GOG achievements as soon as a mod is active, via a
"Modded" flag stored in the save file itself. Once that flag is set, the
common belief — including among authors of other save-editing tools — is
that achievements are blocked for that save **forever**, even after
removing your mods and re-enabling achievements in-game.

That belief is only true if you never touch the save file. The "Modded"
flag can be rewritten directly inside the save package, and that's exactly
what this tool does: scan your saves, spot the flagged ones, repair them
with one click (automatic backup first).

Verified in real conditions: the endgame achievement **Tactician**
unlocked after repairing a save flagged as modded, taken right before the
final boss fight, then finishing the fight on that repaired save. The
incremental achievement **Punch Drunk** also progressed normally (0/20 →
1/20) after repairing a save and continuing to play it — confirming this
restores genuine, ongoing achievement eligibility, not a one-off trick.

**Known cosmetic issue**: BG3 may show a "file corrupted/tampered" warning
after loading a repaired save. This is a harmless side effect of
repackaging the save archive — it has no effect on gameplay, save
integrity, or achievements.

### Installation instructions

1. Download the zip from the Files tab.
2. Extract anywhere.
3. Run BG3FlaggedSaveSavior.exe — no installation, no extra runtime
   required.

### Main features

- **Scan** — detects all your saves, grouped by campaign/character, with a
  clean/flagged indicator per save.
- **Repair** — clears the `Modded` flag on a flagged save with one click,
  with an automatic backup of the original before any change.
- **Rename** — renames a save (in-game display name + file/folder on disk)
  for manual saves. Not available on Quicksave/Autosave slots, since the
  game always computes its own label for those regardless of what's
  stored.

### Requirements

Windows 10/11 (64-bit). No other dependencies — the build is
self-contained (no .NET install needed).

This tool only fixes the save file — it does **not** re-enable
achievements in the game itself. You need a working achievement enabler
already installed and active. Tested with [BG3 Mod Manager (LaughingLeader)
v1.0.12.9](https://github.com/LaughingLeader/BG3ModManager/releases/tag/1.0.12.9),
with its Achievement Enabler option checked, and Script Extender (BG3SE)
installed. Other achievement-enabler mods/tools haven't been tested.

### Shout outs

Built on [LSLib](https://github.com/Norbyte/lslib) by Norbyte (MIT
license), the same library used by Divine.exe and BG3 Mod Manager to
read/write Baldur's Gate 3's save formats.

## Tags

Category: **Utilities**. Suggested tags: `achievements`, `save editor`,
`utility`.
