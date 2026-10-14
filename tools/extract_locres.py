"""Extract Bloodlines 2 .locres files for given cultures and dump them as JSON {ns/key: text}."""
import ctypes, json, os, struct, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pak_index import read_index, decode_entry

PAK = sys.argv[1]; OODLE = sys.argv[2]; OUT = sys.argv[3]; CULTURES = sys.argv[4:]
oodle = ctypes.WinDLL(OODLE)
dec = oodle.OodleLZ_Decompress
dec.restype = ctypes.c_int64
dec.argtypes = [ctypes.c_void_p, ctypes.c_int64, ctypes.c_void_p, ctypes.c_int64] + [ctypes.c_int]*3 + \
               [ctypes.c_void_p, ctypes.c_int64, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_int64, ctypes.c_int]

def read_file(h, e):
    h.seek(e["offset"])
    hdr = h.read(8*3 + 4 + 20)
    _, size, usize, method = struct.unpack_from("<QQQI", hdr)
    if method == 0:
        h.read(5)  # flags + block size are serialized even without compression
        return h.read(usize)
    nb, = struct.unpack("<I", h.read(4))
    blocks = [struct.unpack("<QQ", h.read(16)) for _ in range(nb)]
    flags = h.read(1); bsize, = struct.unpack("<I", h.read(4))
    out = bytearray(); left = usize
    for s, t in blocks:
        h.seek(e["offset"] + s); comp = h.read(t - s)
        n = min(bsize, left) if bsize else left
        raw = ctypes.create_string_buffer(n)
        r = dec(comp, len(comp), raw, n, 1, 0, 0, None, 0, None, None, None, 0, 3)
        if r != n: raise RuntimeError("oodle failed %d/%d" % (r, n))
        out += raw.raw; left -= n
    return bytes(out)

def fstr(b, p):
    n, = struct.unpack_from("<i", b, p); p += 4
    if n == 0: return "", p
    if n < 0: return b[p:p-2*n].decode("utf-16-le").rstrip("\0"), p - 2*n
    return b[p:p+n].decode("latin-1").rstrip("\0"), p + n

def parse_locres(b):
    p = 16; ver = b[p]; p += 1
    soff, = struct.unpack_from("<q", b, p); p += 8
    if ver >= 2: p += 4  # entries count
    strings = []
    q = soff; n, = struct.unpack_from("<i", b, q); q += 4
    for _ in range(n):
        s, q = fstr(b, q)
        if ver >= 2: q += 4
        strings.append(s)
    res = {}
    nns, = struct.unpack_from("<I", b, p); p += 4
    for _ in range(nns):
        if ver >= 2: p += 4
        ns, p = fstr(b, p)
        nk, = struct.unpack_from("<I", b, p); p += 4
        for _ in range(nk):
            if ver >= 2: p += 4
            k, p = fstr(b, p)
            p += 4
            si, = struct.unpack_from("<i", b, p); p += 4
            res[ns + "/" + k] = strings[si]
    return res

files, enc, methods = read_index(PAK)
os.makedirs(OUT, exist_ok=True)
with open(PAK, "rb") as h:
    for c in CULTURES:
        merged = {}
        for path, eo in files.items():
            if not path.endswith(".locres") or ("/%s/" % c) not in path: continue
            table = path.split("/Localization/")[1].split("/")[0]
            for k, v in parse_locres(read_file(h, decode_entry(enc, eo))).items():
                merged[table + "/" + k] = v
        with open(os.path.join(OUT, c + ".json"), "w", encoding="utf-8") as o:
            json.dump(merged, o, ensure_ascii=False, indent=0)
        print(c, len(merged), "strings")
