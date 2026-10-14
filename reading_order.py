# -*- coding: utf-8 -*-
"""Reconstruct the order in which the dialogue is actually read.

Entry IDs are numbered in the order the authors created the nodes, not the order
they are read in: 108/410 sits in the middle of the opening monologue. links.csv
holds the link graph, extracted from the running game by the plugin.

The graph is walked depth-first from the START node (id 0), following the links in
the order the author wrote them. A depth-first walk follows one narrative thread to
its end and then backtracks to the alternatives: translating in that order keeps the
context together, whereas a breadth-first walk would interleave unrelated scenes
sitting at the same depth.

Usage:
    python reading_order.py                 -> summary of every conversation
    python reading_order.py 108             -> reading order of conversation 108
    python reading_order.py 108 --todo      -> only the lines not yet translated
    python reading_order.py 108 --batch 60  -> the first 60 still to do
"""
import csv
import glob
import io
import json
import os
import sys
from collections import defaultdict

HERE = os.path.dirname(os.path.abspath(__file__))
LINKS = os.path.join(HERE, "links.csv")
DUMP = os.path.join(HERE, "dump_en.csv")
BLOCKS = os.path.join(HERE, "blocks")


def read_csv(path):
    with io.open(path, encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def load_graph():
    """conversation -> {entry -> [destination entries, in the original order]}"""
    graph = defaultdict(lambda: defaultdict(list))
    outbound = defaultdict(list)  # conversation -> [conversations reached from it]
    for row in read_csv(LINKS):
        conv, entry = int(row["ConvID"]), int(row["EntryID"])
        dest_conv, dest_entry = int(row["DestConvID"]), int(row["DestEntryID"])
        if dest_conv == conv:
            graph[conv][entry].append(dest_entry)
        else:
            graph[conv][entry].append(None)  # placeholder: the thread leaves here
            if dest_conv not in outbound[conv]:
                outbound[conv].append(dest_conv)
    return graph, outbound


def walk(conversation, graph):
    """Depth-first from START, then any unreachable nodes by id."""
    edges = graph.get(conversation, {})
    seen, order = set(), []
    stack = [0]
    while stack:
        node = stack.pop()
        if node is None or node in seen:
            continue
        seen.add(node)
        order.append(node)
        # Reversed: the stack pops from the top, so the first link stays first.
        for destination in reversed(edges.get(node, [])):
            if destination is not None and destination not in seen:
                stack.append(destination)

    orphans = sorted(set(edges) - seen)
    for node in orphans:
        if node not in seen:
            seen.add(node)
            order.append(node)
    return order, orphans


def load_dump():
    """(conversation, entry) -> [(field, text, actor)] in dump order"""
    lines = defaultdict(list)
    titles = {}
    for row in read_csv(DUMP):
        conv, entry = int(row["ConvID"]), int(row["EntryID"])
        lines[(conv, entry)].append((row["Field"], row["Text"], row["Actor"]))
        titles[conv] = row["Conversation"]
    return lines, titles


def load_done():
    done = set()
    for path in sorted(glob.glob(os.path.join(BLOCKS, "*.json"))):
        with io.open(path, encoding="utf-8") as handle:
            done.update(json.load(handle).keys())
    return done


def main():
    graph, _ = load_graph()
    lines, titles = load_dump()
    done = load_done()

    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    todo_only = "--todo" in sys.argv or "--batch" in sys.argv
    limit = None
    if "--batch" in sys.argv:
        index = sys.argv.index("--batch")
        limit = int(sys.argv[index + 1]) if index + 1 < len(sys.argv) else 50

    if not args:
        print("ConvID    lines  translated  title")
        for conv in sorted(titles):
            count = sum(len(v) for (c, _), v in lines.items() if c == conv)
            translated = sum(1 for k in done if k.startswith("%d/" % conv))
            print("%6d  %7d  %10d  %s" % (conv, count, translated, titles[conv]))
        total = sum(len(v) for v in lines.values())
        print("\ntotal: %d/%d translated (%d%%)"
              % (len(done), total, round(100.0 * len(done) / total)))
        return

    conversation = int(args[0])
    order, orphans = walk(conversation, graph)
    if orphans:
        sys.stderr.write("%d nodes unreachable from START, listed at the end\n"
                         % len(orphans))

    beyond = 0
    shown = 0
    for position, entry in enumerate(order):
        for field, text, actor in lines.get((conversation, entry), []):
            key = "%d/%d/%s" % (conversation, entry, field)
            if todo_only and key in done:
                continue
            if limit is not None and shown >= limit:
                beyond += 1
                continue
            shown += 1
            mark = "" if key in done else "  <-- todo"
            print("[%3d] %s  (%s)%s" % (position, key, actor, mark))
            print("      %s" % text)
    if beyond:
        sys.stderr.write("%d more lines beyond the limit\n" % beyond)


if __name__ == "__main__":
    # The Windows console defaults to cp1252, which mangles the curly quotes in the
    # script; PYTHONIOENCODING does not help under `python -I`.
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    main()
