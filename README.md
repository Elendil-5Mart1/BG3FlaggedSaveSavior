# BG3 Save Savior

A small Windows tool that restores Steam/GOG achievement eligibility on
**Baldur's Gate 3** saves that got flagged as "modded" — a state the
community generally believes is permanent, but isn't.

## The problem it solves

BG3 blocks achievements as soon as a mod is active, via a `Modded` flag
written into the save's metadata (`meta.lsf`, inside the `.lsv` package).
Once that flag is `True` on a save, the common belief (including among
authors of other community tools) is that achievements stay blocked for
that playthrough forever, even after disabling mods and re-enabling the
in-game achievement system (BG3SE + `EnableAchievements`).

In practice this is only true if you don't touch the save file itself. The
`Modded` flag can be rewritten directly inside the save, and BG3 Save
Savior does exactly that: it scans your saves, shows which ones are
flagged, and repairs them in place (with an automatic backup) so
achievements unlock normally again when you keep playing that save.

Verified in real conditions: a one-shot achievement unlocked immediately
after repairing a flagged save, and an incremental achievement ("Punch
Drunk") progressed normally afterwards — confirming the repair restores
genuine, ongoing achievement eligibility, not just a one-time trigger.

## Features

- **Scan** — detects all your saves (grouped by campaign/character) and
  shows which ones are flagged as modded.
- **Repair** — clears the `Modded` flag on a flagged save. The original
  file is always backed up first (never overwritten) to a
  `Story_BG3SaveSavior_Backups/` folder next to your saves.
- **Rename** — renames a save (both the in-game display name and the
  file/folder on disk) for manual saves. Not available on
  Quicksave/Autosave slots, since the game ignores their stored name and
  always computes its own label — this is a game engine limitation, not a
  tool limitation.

## Known cosmetic issue

After a repair or rename, the game may show a "file corrupted/tampered"
warning the next time that save is loaded. This is a harmless side effect
of repackaging the save archive (a known, unresolved issue in the
underlying library — see [Norbyte/lslib#292](https://github.com/Norbyte/lslib/issues/292),
which reproduces the same warning on a save that was extracted and
repacked with *zero* modifications). It has **no effect on gameplay,
save integrity, or achievements** — confirmed in testing.

## Download

Grab the latest build from the [Releases](../../releases) page. Unzip and
run `BG3SaveSavior.exe` — no installation required.

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
cd src
dotnet run
```

Release build:

```
cd src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## How it works

Built on top of [LSLib](https://github.com/Norbyte/lslib) by Norbyte (MIT
license), the same library used by `Divine.exe` and BG3 Mod Manager to
read/write Larian's proprietary save formats. BG3 Save Savior uses it
in-process (no subprocess calls) to:

1. Back up the original `.lsv` save package.
2. Unpack it, edit the relevant attribute in `meta.lsf` (or the save name
   in `SaveInfo.json` for renaming) via LSLib's resource API.
3. Repack the `.lsv` and replace the original.

Your save folder is auto-detected under
`%LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\PlayerProfiles\*\Savegames\Story`,
with a "Browse" button to point elsewhere if needed.

## License

MIT — see [LICENSE](LICENSE). Bundles [LSLib](https://github.com/Norbyte/lslib)
by Norbyte, also MIT-licensed (see `lib/LICENSE-LSLib.txt`).

## Disclaimer

Not affiliated with Larian Studios. Use at your own risk — always keep
backups of your saves (this tool makes its own automatically, but a
second copy never hurts).
