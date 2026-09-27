"""Update the Unity package version and export a .unitypackage without samples/tests."""

import argparse
import json
import re
import subprocess
import tarfile
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
PACKAGE_ROOT = Path("Assets/GValidator")
EXCLUDED_DIRECTORIES = {"examples", "test", "tests", "tests~"}
EXCLUDED_FILES = {".npmignore"}
VERSION_PATTERN = re.compile(
    r"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)"
    r"(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?"
    r"(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$"
)
BASE_VERSION_PATTERN = re.compile(r"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$")


def next_preview_version(base_version: str) -> str:
    if not BASE_VERSION_PATTERN.fullmatch(base_version):
        raise ValueError(f"Invalid base version: {base_version}")

    tag_prefix = f"v{base_version}-preview."
    remote_tags = subprocess.check_output(
        ["git", "ls-remote", "--tags", "origin", f"refs/tags/{tag_prefix}*"],
        cwd=ROOT,
        text=True,
    )

    highest_preview = 0
    for line in remote_tags.splitlines():
        tag = line.split("\t", 1)[1].removeprefix("refs/tags/")
        suffix = tag.removeprefix(tag_prefix)
        if tag.startswith(tag_prefix) and suffix.isdecimal() and not suffix.startswith("0"):
            highest_preview = max(highest_preview, int(suffix))

    return f"{base_version}-preview.{highest_preview + 1}"


def update_version(version: str) -> None:
    if not VERSION_PATTERN.fullmatch(version):
        raise ValueError(f"Invalid package version: {version}")

    manifest_path = ROOT / PACKAGE_ROOT / "package.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    manifest["version"] = version
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


def included(path: Path) -> bool:
    asset_path = path.with_name(path.name.removesuffix(".meta"))
    if asset_path.name.lower() in EXCLUDED_FILES:
        return False

    relative_parts = asset_path.relative_to(PACKAGE_ROOT).parts
    return not any(
        part.removesuffix(".meta").lower() in EXCLUDED_DIRECTORIES
        for part in relative_parts
    )


def guid_from_meta(content: bytes, path: Path) -> str:
    match = re.search(rb"(?m)^guid: ([0-9a-f]{32})\s*$", content)
    if match is None:
        raise ValueError(f"Unity metadata has no GUID: {path}")

    return match.group(1).decode("ascii")


def add_bytes(archive: tarfile.TarFile, name: str, content: bytes) -> None:
    import io

    info = tarfile.TarInfo(name)
    info.size = len(content)
    info.mode = 0o644
    archive.addfile(info, io.BytesIO(content))


def add_asset(
    archive: tarfile.TarFile, path: Path, meta: bytes, content: bytes | None
) -> None:
    guid = guid_from_meta(meta, path.with_name(path.name + ".meta"))
    add_bytes(archive, f"{guid}/pathname", path.as_posix().encode("utf-8"))
    add_bytes(archive, f"{guid}/asset.meta", meta)
    if content is not None:
        add_bytes(archive, f"{guid}/asset", content)


def build_unitypackage(output: Path) -> None:
    tracked = subprocess.check_output(
        ["git", "ls-files", "-z", "--", str(PACKAGE_ROOT), "Assets/GValidator.meta"],
        cwd=ROOT,
    )
    paths = {Path(raw.decode("utf-8")) for raw in tracked.split(b"\0") if raw}
    paths = {path for path in paths if path == Path("Assets/GValidator.meta") or included(path)}

    package_files = sorted(path for path in paths if path.suffix != ".meta")
    meta_files = sorted(path for path in paths if path.suffix == ".meta")

    for path in package_files:
        meta_path = path.with_name(path.name + ".meta")
        if meta_path not in paths:
            raise ValueError(f"Missing Unity metadata: {meta_path}")

    output.parent.mkdir(parents=True, exist_ok=True)
    with tarfile.open(output, "w:gz") as archive:
        for meta_path in meta_files:
            asset_path = meta_path.with_name(meta_path.name.removesuffix(".meta"))
            meta = (ROOT / meta_path).read_bytes()
            content = (ROOT / asset_path).read_bytes() if asset_path in paths else None
            add_asset(archive, asset_path, meta, content)



def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("version").add_argument("version")
    commands.add_parser("build").add_argument("output", type=Path)
    commands.add_parser("next-preview").add_argument("base_version")
    args = parser.parse_args()

    if args.command == "version":
        update_version(args.version)
    elif args.command == "build":
        build_unitypackage(args.output.resolve())
    else:
        print(next_preview_version(args.base_version))


if __name__ == "__main__":
    main()
