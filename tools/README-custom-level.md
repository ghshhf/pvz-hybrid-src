# PVZ Hybrid custom level workflow

The game data is NOT encrypted. It lives in `植物大战僵尸杂交版发布版0.31.0.Csharp.pck`
(~519 MB) as plain JSON + Godot `.tres` resources. This makes real level modding possible.

## Toolchain

- `gdre_tools.exe` (Godot RE Tools v2.7.4) at `D:\tools\gdre\gdre_tools.exe`
  - Download: https://github.com/GDRETools/gdsdecomp/releases (GDRE_tools-v2.7.4-windows.zip)
  - 7-Zip: `C:\Users\123\scoop\apps\7zip\26.01\7z.exe`
- Game dir: `D:\pvz-game\game-0.31\植物大战僵尸杂交重制版\`
- Extracted data: `D:\pvz-game\extracted\`

## Key facts

- 28,089 files in PCK, `Verified ... no errors` => unencrypted.
- Level registry: `res://Asset/Config/Level/LevelResource.json`
  - structure: `Challenge.Chapter[].Level[]` with `{OpenKey, SaveKey, UnlockImage, Level}`
  - top-level modes: Adventure / Challenge / Survival / Puzzle / MiniGames / IZM2 / HybridPark
- Level bodies: `res://Asset/Config/Level/TowerDefense/<Mode>/<Set>/<Name>.json` + matching `.tres`
  - JSON holds Name / Description / LevelName / Map / BGM / Reward / PacketBank /
    SunManager / WaveManager (Wave[].Spawn[].{Line,Num,Zombie}) / Event / PreSpawn
  - `.tres` is the Godot Resource wrapper; contains a unique `uid://` header and
    an ext_resource pointing at the JSON. Give a new level a fresh uid.

## Build a custom level (repeatable)

1. Extract a template:
   ```
   gdre_tools.exe --headless --extract=<pck> --output=D:\pvz-game\extracted \
     --include="Asset/Config/Level/LevelResource.json" \
     --include="Asset/Config/Level/TowerDefense/Challenge/Diamond/Challenge_Level_Diamond10_1.json" \
     --include="Asset/Config/Level/TowerDefense/Challenge/Diamond/Challenge_Level_Diamond10_1.tres"
   ```
2. Run `python E:\xmanbian\tools_and_notes\make_custom_level.py` — clones the template
   into `Challenge_Level_Custom1_1`, builds a 20-wave zombie-tide, registers it in
   the catalog (chapter 1, position 2, OpenKey empty = immediately reachable).
3. Patch into a NEW pck (never overwrite the original):
   ```
   gdre_tools.exe --headless --pck-patch=<original.pck> \
     --patch-file=<local.json>=res://Asset/Config/Level/.../Challenge_Level_Custom1_1.json \
     --patch-file=<local.tres>=res://Asset/Config/Level/.../Challenge_Level_Custom1_1.tres \
     --patch-file=<local_catalog.json>=res://Asset/Config/Level/LevelResource.json \
     --output=D:\pvz-game\pvz-custom.pck
   ```
4. Install into a COPY of the game dir (rename pck to the game's exact filename),
   verify: `--headless --quit-after 200` must log `Full gameplay resources ready`
   and 0 real errors. Verify with `--list-files` that the 2 new files are present,
   and re-extract the catalog to confirm the new entry.

## Verified result (2026-11-24)

- Custom level `Challenge_Level_Custom1_1` (20-wave zombie tide, 5000 start sun,
  preset plant bank, 9999 coin reward) injected successfully.
- New pck: 28,091 files (original 28,089) — the two new level files.
- Game headless load: resources ready, 0 errors.
- Original pck backed up at `*.pck.bak` (519.5 MB); test copy under
  `D:\pvz-game\game-custom-test\` so the running game is untouched.

## Caveats

- `.tres` uid must be unique within the project; the generator assigns `uid://c2pvzcustom1x9`.
- The catalog edit reuses an existing `UnlockImage` uid so no new texture is needed.
- Levels use localized name KEYS (e.g. `CHALLENGE_LEVEL_DIALMONDNAME_10`); a brand-new
  key would show raw text unless added to the localization csv. Reusing an existing key
  avoids that.
- To make a custom level reachable in a fresh game, the save must reflect the catalog;
  OpenKey="" means no prerequisite level.
