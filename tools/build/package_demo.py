#!/usr/bin/env python3
"""Package a successful 0.5 demo build with platform checks, manifest and SHA-256."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--platform', required=True, choices=('windows', 'linux', 'mac'))
    parser.add_argument('--source', type=Path)
    parser.add_argument('--name')
    parser.add_argument('--allow-development', action='store_true')
    args = parser.parse_args()
    source = args.source or ROOT / 'builds/Demo' / args.platform
    info = json.loads((source / 'build-info.json').read_text())
    if info.get('platform') != args.platform or info.get('unity') != '6000.3.25f1' or info.get('version') != '0.5.0':
        parser.error('Missing or mismatched demo build metadata.')
    if info.get('development') and not args.allow_development:
        parser.error('Development builds require --allow-development.')
    default = 'Farmer-0.5.0-' + args.platform + ('-development' if info.get('development') else '')
    name = args.name or default
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9_.-]*', name):
        parser.error('Use an ASCII package name without path separators.')
    executable = source / {'windows': 'Farmer.exe', 'linux': 'Farmer.x86_64', 'mac': 'Farmer.app/Contents/MacOS/Farmer'}[args.platform]
    header = executable.read_bytes()[:4096]
    if args.platform == 'windows':
        offset = struct.unpack_from('<I', header, 0x3C)[0]
        if header[:2] != b'MZ' or header[offset:offset+6] != b'PE\0\0\x64\x86':
            parser.error('Expected Windows x64 player.')
    elif args.platform == 'linux' and header[:5] != b'\x7fELF\x02':
        parser.error('Expected Linux 64-bit ELF player.')
    elif args.platform == 'mac' and header[:4] not in (b'\xcf\xfa\xed\xfe', b'\xca\xfe\xba\xbe'):
        parser.error('Expected macOS Mach-O player.')
    required = {'windows': ('UnityPlayer.dll', 'Farmer_Data/globalgamemanagers', 'MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll'),
                'linux': ('UnityPlayer.so', 'Farmer_Data/globalgamemanagers'),
                'mac': ('Farmer.app/Contents/Info.plist',)}[args.platform]
    for relative in required:
        if not (source / relative).is_file():
            parser.error('Missing runtime file: ' + relative)
    files = sorted(p for p in source.rglob('*') if p.is_file() and not any(
        'DoNotShip' in part or part.endswith('_BackUpThisFolder_ButDontShipItWithYourGame') for part in p.parts))
    manifest = {p.relative_to(source).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
    output = ROOT / 'builds/Releases' / (name + '.zip')
    output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(output, 'x', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for p in files:
            archive.write(p, name + '/' + p.relative_to(source).as_posix())
        archive.writestr(name + '/MANIFEST.json', json.dumps(manifest, indent=2))
        archive.writestr(name + '/BENI_OKU.txt', (ROOT / 'docs/playtest/DEMO_README.txt').read_text())
    with zipfile.ZipFile(output) as archive:
        bad = archive.testzip()
        if bad:
            raise SystemExit('Archive CRC failed: ' + bad)
    digest = hashlib.sha256(output.read_bytes()).hexdigest()
    output.with_suffix('.zip.sha256').write_text(digest + '  ' + output.name + '\n')
    print(f'PACKAGE_OK: {output}\n{len(files)} files; {output.stat().st_size / 1024**2:.1f} MiB\nSHA256: {digest}')


if __name__ == '__main__':
    main()
