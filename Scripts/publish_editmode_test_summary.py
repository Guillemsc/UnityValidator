#!/usr/bin/env python3
"""Write Unity EditMode JUnit results to the GitHub Actions run summary."""

import os
import xml.etree.ElementTree as ET
from pathlib import Path


def main() -> None:
    results_path = Path(os.environ["TEST_RESULTS_PATH"])
    summary_path = Path(os.environ["GITHUB_STEP_SUMMARY"])
    outcome = os.environ.get("TEST_OUTCOME", "unknown")

    with summary_path.open("a", encoding="utf-8") as summary:
        summary.write("## Unity EditMode test results\n\n")

        if not results_path.is_file():
            summary.write(
                f"Test step outcome: **{outcome}**. "
                f"JUnit XML was not found at `{results_path}`.\n"
            )
            return

        try:
            root = ET.parse(results_path).getroot()
        except (ET.ParseError, OSError) as error:
            summary.write(
                f"Test step outcome: **{outcome}**. "
                f"Could not read JUnit XML: `{error}`.\n"
            )
            return

        counts = {
            name: root.attrib.get(name, "0")
            for name in ("total", "passed", "failed", "skipped", "inconclusive")
        }
        summary.write(f"Test step outcome: **{outcome}**\n\n")
        summary.write("| Total | Passed | Failed | Skipped | Inconclusive |\n")
        summary.write("| ---: | ---: | ---: | ---: | ---: |\n")
        summary.write(
            "| {total} | {passed} | {failed} | {skipped} | {inconclusive} |\n".format(**counts)
        )

        failed_tests = [
            test_case
            for test_case in root.findall(".//test-case")
            if test_case.attrib.get("result", "").lower() in {"failed", "error"}
        ]
        if failed_tests:
            summary.write("\n### Failed tests\n\n")
            for test_case in failed_tests:
                name = test_case.attrib.get("fullname") or test_case.attrib.get("name", "Unknown test")
                name = name.replace("|", "\\|").replace("\n", " ")
                details = test_case.find("failure")
                if details is None:
                    details = test_case.find("error")
                message = (
                    " ".join("".join(details.itertext()).split())
                    if details is not None
                    else "No failure message in XML."
                )
                message = message.replace("|", "\\|").replace("\n", " ")
                if len(message) > 1000:
                    message = message[:997] + "..."
                summary.write(f"- **`{name}`** — {message}\n")


if __name__ == "__main__":
    main()
