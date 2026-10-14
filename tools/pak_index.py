"""Minimal reader for an unencrypted UE5 pak (version 11) index."""
import os, struct, sys

def fstring(b, p):
    n, = struct.unpack_from("<i", b, p); p += 4
    if n == 0: return "", p
    if n < 0:
        s = b[p:p-2*n].decode("utf-16-le").rstrip("\0"); return s, p - 2*n
    return b[p:p+n].decode("latin-1").rstrip("\0"), p + n

def read_index(path):
    size = os.path.getsize(path)
    with open(path, "rb") as h:
        h.seek(size - 300); tail = h.read()
        i = tail.rfind(struct.pack("<I", 0x5A6F12E1))
        ver, ioff, isz = struct.unpack_from("<IQQ", tail, i + 4)
        methods = [tail[i+44+32*k:i+44+32*(k+1)].split(b"\0")[0].decode() for k in range(5)]
        h.seek(ioff); idx = h.read(isz)
        p = 0
        mount, p = fstring(idx, p)
        n, = struct.unpack_from("<i", idx, p); p += 4
        p += 8
        has_ph, = struct.unpack_from("<i", idx, p); p += 4
        if has_ph: p += 8 + 8 + 20
        has_fd, = struct.unpack_from("<i", idx, p); p += 4
        fd_off, fd_size = struct.unpack_from("<QQ", idx, p); p += 8 + 8 + 20
        enc_size, = struct.unpack_from("<i", idx, p); p += 4
        enc = idx[p:p+enc_size]
        h.seek(fd_off); fd = h.read(fd_size)
    files = {}
    q = 0
    ndirs, = struct.unpack_from("<i", fd, q); q += 4
    for _ in range(ndirs):
        d, q = fstring(fd, q)
        nf, = struct.unpack_from("<i", fd, q); q += 4
        for _ in range(nf):
            f, q = fstring(fd, q)
            eo, = struct.unpack_from("<i", fd, q); q += 4
            files[mount + d + f] = eo
    return files, enc, methods

def decode_entry(enc, eo):
    v, = struct.unpack_from("<I", enc, eo); p = eo + 4
    bs = (v & 0x3f)
    if bs == 0x3f:
        bs, = struct.unpack_from("<I", enc, p); p += 4
    else: bs <<= 11
    nblocks = (v >> 6) & 0xffff
    encrypted = (v >> 22) & 1
    method = (v >> 23) & 0x3f
    def rd(safe):
        nonlocal p
        if safe: x, = struct.unpack_from("<I", enc, p); p += 4
        else: x, = struct.unpack_from("<Q", enc, p); p += 8
        return x
    off = rd((v >> 31) & 1)
    usize = rd((v >> 30) & 1)
    csize = rd((v >> 29) & 1) if method else usize
    return dict(offset=off, usize=usize, csize=csize, method=method, nblocks=nblocks,
                block_size=bs, encrypted=encrypted)

if __name__ == "__main__":
    files, enc, methods = read_index(sys.argv[1])
    print("methods", methods, "files", len(files))
    for k, eo in files.items():
        if any(s in k.lower() for s in sys.argv[2:]):
            print(k, decode_entry(enc, eo))
