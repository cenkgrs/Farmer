#!/usr/bin/env python3
"""Package a successful Windows player build; no third-party Python packages needed."""
from pathlib import Path
import argparse
import hashlib
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--name", default="Farmer-0.1.0-Windows-x64")
    args = parser.parse_args()
    if not args.name or any(c not in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_." for c in args.name):
        parser.error("Use an ASCII filename with letters, digits, dash, underscore or dot.")
    build = ROOT / "builds/Windows"
    required = (
        "Farmer.exe", "UnityPlayer.dll", "UnityCrashHandler64.exe",
        "Farmer_Data/globalgamemanagers", "Farmer_Data/Managed/Farmer.Runtime.dll",
        "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll",
    )
    for relative in required:
        path = build / relative
        if not path.is_file() or not path.stat().st_size:
            raise SystemExit(f"Missing build file: {relative}")
    executable = (build / "Farmer.exe").read_bytes()
    pe_offset = struct.unpack_from("<I", executable, 0x3C)[0]
    if executable[:2] != b"MZ" or executable[pe_offset:pe_offset + 6] != b"PE\0\0\x64\x86":
        raise SystemExit("Farmer.exe is not a Windows x64 PE executable.")
    # Only player runtime files; omit Unity's sibling backup/debug folders.
    files = [p for p in build.iterdir() if p.is_file() and p.suffix.lower() in (".exe", ".dll")]
    for folder in ("Farmer_Data", "MonoBleedingEdge", "D3D12"):
        files.extend(p for p in (build / folder).rglob("*") if p.is_file())
    output = ROOT / "builds/Releases" / f"{args.name}.zip"
    output.parent.mkdir(parents=True, exist_ok=True)
    if output.exists():
        raise SystemExit(f"Package already exists; choose a new --name: {output}")
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for path in sorted(files):
            archive.write(path, f"{args.name}/{path.relative_to(build).as_posix()}")
        readme = (ROOT / "docs/playtest/WINDOWS_README.txt").read_text()
        archive.writestr(f"{args.name}/BENI_OKU.txt", readme.replace("\n", "\r\n").encode("utf-8-sig"))
    with zipfile.ZipFile(output) as archive:
        bad = archive.testzip()
        if bad:
            raise SystemExit(f"Archive CRC check failed: {bad}")
        count = len(archive.namelist())
    digest = hashlib.sha256(output.read_bytes()).hexdigest()
    output.with_suffix(".zip.sha256").write_text(f"{digest}  {output.name}\n")
    print(f"PACKAGE_OK: {output}\nFiles: {count}; size: {output.stat().st_size / 1024**2:.1f} MiB\nSHA256: {digest}")


if __name__ == "__main__":
    main()
