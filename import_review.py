# -*- coding: utf-8 -*-
"""Bring a proofreader's corrected review_es.csv back into the translation blocks.

review_es.csv is the sheet the plugin writes on a dump run: one row per line, with the
English and the Spanish side by side. A proofreader corrects the ES column and sends the
file back; this finds every row whose Spanish differs from the blocks and rewrites it in
the block that owns that line, so the blocks stay the single source of es.csv.

Usage:
    python import_review.py review_es.csv            -> list the changes, write nothing
    python import_review.py review_es.csv --write    -> apply them to blocks/*.json
Then run apply.py as usual to check and rebuild es.csv.
"""
import csv, glob, io, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load_blocks():
    owner, blocks = {}, {}
    for path in sorted(glob.glob(os.path.join(HERE, "blocks", "*.json"))):
        blocks[path] = json.load(io.open(path, encoding="utf-8"))
        for key in blocks[path]:
            owner[key] = path
    return owner, blocks


def main(args):
    write = "--write" in args
    sheet = [a for a in args if a != "--write"][0]
    owner, blocks = load_blocks()

    # Spreadsheets may save with or without a BOM; utf-8-sig reads both.
    rows = list(csv.reader(io.open(sheet, encoding="utf-8-sig", newline="")))
    header, rows = rows[0], rows[1:]
    changes, unknown, emptied = [], [], []
    for row in rows:
        if len(row) < 4:
            continue
        key = "/".join(row[:3])
        new = row[-1]
        if key not in owner:
            unknown.append(key)
            continue
        if not new.strip():
            emptied.append(key)  # a blank cell is not taken as "delete this line"
            continue
        if new != blocks[owner[key]][key]:
            changes.append((key, blocks[owner[key]][key], new))

    for key, old, new in changes:
        print(f"{key}\n  - {old}\n  + {new}")
    print(f"\n{len(changes)} changed lines out of {len(rows)} rows")
    if unknown:
        print(f"{len(unknown)} rows with keys not in the blocks (ignored), e.g. {unknown[:3]}")
    if emptied:
        print(f"{len(emptied)} rows left blank (ignored)")

    if not write or not changes:
        return
    touched = set()
    for key, _, new in changes:
        blocks[owner[key]][key] = new
        touched.add(owner[key])
    for path in touched:
        with io.open(path, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(blocks[path], handle, ensure_ascii=False, indent=0)
            handle.write("\n")
    print(f"written to {len(touched)} block files; now run apply.py")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main(sys.argv[1:])
