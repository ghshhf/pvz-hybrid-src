# -*- coding: utf-8 -*-
"""Build a custom challenge level for PVZ Hybrid 0.31 from the extracted template.

Reads the extracted original level JSON, creates a new level with unique waves,
registers it in LevelResource.json, and copies the .tres with a fresh uid.
"""
import json
import os
import shutil
import re

BASE = r"D:\pvz-game\extracted"
OUT = r"D:\pvz-game\custom_level"
LEVEL_DIR = os.path.join(BASE, "Asset", "Config", "Level", "TowerDefense", "Challenge", "Diamond")
SRC_JSON = os.path.join(LEVEL_DIR, "Challenge_Level_Diamond10_1.json")
SRC_TRES = os.path.join(LEVEL_DIR, "Challenge_Level_Diamond10_1.tres")
CATALOG = os.path.join(BASE, "Asset", "Config", "Level", "LevelResource.json")

NEW_ID = "Challenge_Level_Custom1_1"
NEW_JSON = os.path.join(LEVEL_DIR, NEW_ID + ".json")
NEW_TRES = os.path.join(LEVEL_DIR, NEW_ID + ".tres")

os.makedirs(OUT, exist_ok=True)

with open(SRC_JSON, "r", encoding="utf-8") as f:
    lvl = json.load(f)

# --- Design: "Zombie Tide" custom challenge ---
lvl["Name"] = NEW_ID
lvl["Description"] = "CUSTOM_ZOMBIE_TIDE"
lvl["LevelName"] = "CHALLENGE_LEVEL_DIALMONDNAME_10"   # reuse existing localized name key
lvl["LevelNumber"] = 1
lvl["NextLevel"] = ""
lvl["Reward"] = {"RewardFirst": "9999", "RewardType": "Coin"}
lvl["SunManager"] = {
    "Begin": 5000,
    "MovingMethod": "Land",
    "Open": True,
    "SpawnInterval": 5,
    "SpawnNum": 100,
}
# Keep the preset plant bank from template, but build an escalating zombie-tide wave set.
zombie_types = [
    "ZombieNormalCone",
    "ZombieNormalGatlingPea",
    "ZombieFootball",
    "ZombieImpScaredy",
    "ZombieGargantuar",
    "ZombieZamboni",
]
waves = []
for i in range(20):
    spawns = []
    # escalating count, mix of types, all rows (-1 = random)
    total = 5 + i * 2
    for k in range(min(3 + i // 5, len(zombie_types))):
        zt = zombie_types[(i + k) % len(zombie_types)]
        num = max(1, total // (3 + i // 5))
        spawns.append({"Line": -1, "Num": num, "Zombie": zt})
    waves.append({
        "Event": [],
        "DynamicPlantfood": [0, 0, 0, 0, 0, 0, 0],
        "Spawn": spawns,
    })
lvl["WaveManager"]["Wave"] = waves
lvl["WaveManager"]["FlagWaveInterval"] = 5
lvl["WaveManager"]["MaxNextWaveHealthPercentage"] = 0.5
lvl["WaveManager"]["MinNextWaveHealthPercentage"] = 0.3

with open(NEW_JSON, "w", encoding="utf-8") as f:
    json.dump(lvl, f, ensure_ascii=False, indent="\t")

# --- .tres: copy template, replace JSON path and give a fresh uid ---
with open(SRC_TRES, "r", encoding="utf-8") as f:
    tres = f.read()
tres = tres.replace(
    "Challenge_Level_Diamond10_1.json", NEW_ID + ".json"
)
# fresh uid for the resource header (13 chars base-32-ish, unique)
old_uid = re.search(r'uid="(uid://[a-z0-9]+)"', tres).group(1)
new_uid = "uid://c2pvzcustom1x9"
tres = tres.replace(old_uid, new_uid, 1)
with open(NEW_TRES, "w", encoding="utf-8") as f:
    f.write(tres)

# --- Register in LevelResource.json: add to Challenge chapter 1 level list ---
with open(CATALOG, "r", encoding="utf-8") as f:
    catalog = json.load(f)
ch1 = catalog["Challenge"]["Chapter"][0]
levels = ch1["Level"]
entry = {
    "OpenKey": "",
    "SaveKey": NEW_ID,
    "UnlockImage": "uid://btnykfqf83xvy",   # reuse an existing unlocked image uid
    "Level": {},
}
# insert after the first level so it's immediately reachable
if not any(x.get("SaveKey") == NEW_ID for x in levels):
    levels.insert(1, entry)
with open(CATALOG, "w", encoding="utf-8") as f:
    json.dump(catalog, f, ensure_ascii=False, indent=2)

print("NEW_JSON:", NEW_JSON, os.path.getsize(NEW_JSON))
print("NEW_TRES:", NEW_TRES, os.path.getsize(NEW_TRES))
print("CATALOG updated:", CATALOG)
print("UID:", old_uid, "->", new_uid)
print("WAVES:", len(waves))
