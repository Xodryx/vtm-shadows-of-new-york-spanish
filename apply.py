# -*- coding: utf-8 -*-
"""Merge translation blocks into es.csv, keeping the ones already applied.

Usage:  python apply.py blocks/*.json
        python apply.py --check blocks/block_114a.json   -> only report, write nothing
A block file is {"ConvID/EntryID/Field": "Spanish text", ...}
"""
import csv, io, json, os, re, sys

PLUGIN_DIR = (r"C:\Program Files (x86)\Steam\steamapps\common"
              r"\Vampire The Masquerade - Shadows of New York\BepInEx\plugins\SoNY-ESP")
HERE = os.path.dirname(os.path.abspath(__file__))
SOURCE = os.path.join(HERE, "dump_en.csv")
TARGET = os.path.join(PLUGIN_DIR, "es.csv")
GLOSSARY = os.path.join(HERE, "glossary_es.json")

MARKER = re.compile(r"\[(\d+);([^\]]+)\]")

# Quotes and punctuation sit inside the marker in the original too
# ([27;"Kindred."]), so only the term itself is compared.
TRIMMED = '“”«»"\'.,’'



def load_csv(path, key_columns=(0, 1, 2), text_column=5):
    if not os.path.exists(path):
        return {}
    rows = list(csv.reader(io.open(path, encoding="utf-8-sig", newline="")))
    return {"/".join(r[i] for i in key_columns): r[text_column]
            for r in rows[1:] if len(r) > text_column}


def check(source, translations):
    """Automatic checks before writing: markers, tags, length, phantom rows."""
    glossary = json.load(io.open(GLOSSARY, encoding="utf-8"))
    problems = []
    for key, spanish in translations.items():
        english = source.get(key)
        if english is None:
            problems.append(f"{key}: key does not exist in the dump")
            continue
        if not spanish.strip():
            problems.append(f"{key}: empty translation")
            continue

        english_markers = MARKER.findall(english)
        spanish_markers = MARKER.findall(spanish)
        english_ids = sorted(i for i, _ in english_markers)
        spanish_ids = sorted(i for i, _ in spanish_markers)
        if english_ids != spanish_ids:
            problems.append(f"{key}: glossary markers {english_ids} -> {spanish_ids}")

        # A handful of lines break inside the text box on purpose.
        if english.count("\n") != spanish.count("\n"):
            problems.append(f"{key}: {english.count(chr(10))} line breaks -> "
                            f"{spanish.count(chr(10))}")

        # Only constrain a marker when the English term inside it has a known
        # official rendering. Nicknames ([57;Degenerate] for a Toreador) are free
        # to translate: checking the ID alone would reject them.
        spanish_by_id = dict(spanish_markers)
        for marker_id, english_term in english_markers:
            entries = glossary.get(marker_id, {})
            english_term = english_term.strip(TRIMMED)
            official = entries.get(english_term)
            if official is None or marker_id not in spanish_by_id:
                continue
            # Gendered or alternative forms recorded as "Childe (m)" count as the
            # same rendering; anything else does not.
            accepted = {official}
            accepted.update(v for k, v in entries.items()
                            if k.startswith(english_term + " ("))
            term = spanish_by_id[marker_id]
            if term.strip(TRIMMED).lower() not in {a.strip(TRIMMED).lower() for a in accepted}:
                problems.append(
                    f"{key}: [{marker_id};{term}] is not the official rendering "
                    f"of '{english_term}' ({sorted(accepted)})")

        # A percentage says nothing about short lines ("...Sigh." -> "...Suspiro."
        # is +38% but it is 11 characters). What counts is the risk of overflowing
        # the box: the longest line in the game is 228 characters and wraps to three.
        if len(english) >= 40 and len(spanish) > len(english) * 1.35:
            problems.append(
                f"{key}: +{round((len(spanish)/len(english)-1)*100)}% longer")
        if len(spanish) > 300:
            problems.append(f"{key}: {len(spanish)} characters, may overflow the box")
    return problems


def main(args):
    check_only = "--check" in args
    block_files = [a for a in args if a != "--check"]
    source = load_csv(SOURCE)
    existing = load_csv(TARGET)

    incoming = {}
    for path in block_files:
        incoming.update(json.load(io.open(path, encoding="utf-8")))

    problems = check(source, incoming)
    for problem in problems:
        print("  ! " + problem)
    if check_only:
        print(f"{len(incoming)} lines checked, {len(problems)} warnings (nothing written)")
        return

    existing.update(incoming)

    rows = list(csv.reader(io.open(SOURCE, encoding="utf-8-sig", newline="")))
    out = [rows[0]]
    for row in rows[1:]:
        key = f"{row[0]}/{row[1]}/{row[2]}"
        if existing.get(key, "").strip():
            out.append([row[0], row[1], row[2], row[3], row[4], existing[key]])

    with io.open(TARGET, "w", encoding="utf-8-sig", newline="") as handle:
        csv.writer(handle, lineterminator="\n").writerows(out)

    total = len(rows) - 1
    print(f"\n{len(incoming)} lines in these blocks")
    print(f"total: {len(out)-1}/{total} lines ({100*(len(out)-1)//total}%)")
    if problems:
        print(f"{len(problems)} warnings to check")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main(sys.argv[1:])
