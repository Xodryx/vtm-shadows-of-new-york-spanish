# -*- coding: utf-8 -*-
"""Write ui_es.csv: the interface strings the game has no official Spanish for.

Shadows of New York already ships a Spanish column for its menus, actor names, traits,
dictionary titles and about half of the quests; the plugin only falls back to this
file where that column is empty. So ui_es.csv holds just the gaps - mostly dictionary
descriptions - translated by hand in ui_manual_es.json.

Usage:  python build_ui.py
"""
import csv, io, json, os, sys

PLUGIN_DIR = (r"C:\Program Files (x86)\Steam\steamapps\common"
              r"\Vampire The Masquerade - Shadows of New York\BepInEx\plugins\SoNY-ESP")
HERE = os.path.dirname(os.path.abspath(__file__))
DUMP = os.path.join(HERE, "dump_ui_en.csv")
MANUAL = os.path.join(HERE, "ui_manual_es.json")
TARGET = os.path.join(PLUGIN_DIR, "ui_es.csv")


def main():
    rows = list(csv.DictReader(io.open(DUMP, encoding="utf-8-sig", newline="")))
    manual = json.load(io.open(MANUAL, encoding="utf-8"))

    gaps = {r["Type"] + "/" + r["textId"]: r for r in rows
            if r["EN"].strip() and not r["ES"].strip()}
    out = [["Type", "textId", "Text"]]
    for key, row in gaps.items():
        if key in manual:
            out.append([row["Type"], row["textId"], manual[key]])

    missing = sorted(set(gaps) - set(manual))
    unknown = sorted(set(manual) - set(gaps))
    with io.open(TARGET, "w", encoding="utf-8-sig", newline="") as handle:
        csv.writer(handle, lineterminator="\n").writerows(out)

    print(f"{len(out) - 1}/{len(gaps)} gaps filled -> {TARGET}")
    for key in missing:
        print("  missing: " + key)
    for key in unknown:
        print("  ! not a gap (official Spanish exists or wrong key): " + key)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
