[![openupm](https://img.shields.io/npm/v/com.guillemsc.gvalidator?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.guillemsc.gvalidator)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![Tests](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml/badge.svg)](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml)

# GValidator for Unity

<img width="1640" height="856" alt="GValidator-Banner" src="https://github.com/user-attachments/assets/7ac37688-2e0d-46c2-861c-11062a5b9d0b" />

GValidator checks your Unity project for problems such as missing scripts, broken UnityEvent listeners, and shader errors. Run it in the Editor to find the affected objects, or in continuous integration (CI) to catch problems before merging changes.

**You can use the built-in checks without writing any code.** Add attributes or custom rules when you want to check requirements specific to your game.

- **Check what you need:** the whole project, selected scenes, one folder, or one object.
- **Find and fix issues:** filter results by severity and inspect the object and path for each finding.
- **Share team rules:** configure ignored folders, enabled validators, and accepted exceptions.
- **Automate checks:** run in Unity batch mode and export JUnit XML reports for CI.

**Start here:** [Install GValidator](#-installation), then [run your first check](#-quick-start).

<img width="1078" height="704" alt="GVAlidator-Window" src="https://github.com/user-attachments/assets/fdc4e687-b64a-4e24-809f-ec77040196cc" />

## Contents

- [📋 Requirements](#-requirements)
- [📦 Installation](#-installation)
- [🚀 Quick start](#-quick-start)
- [🖥 Using the validation window](#-using-the-validation-window)
- [🔍 Validating an asset or folder](#-validating-an-asset-or-folder)
- [🤝 Sharing project settings](#-sharing-project-settings)
- [📁 Ignoring folders](#-ignoring-folders)
- [🔕 Suppressing individual findings](#-suppressing-individual-findings)
- [✅ Built-in validation rules](#-built-in-validation-rules)
- [🛠 Adding validation to your project](#-adding-validation-to-your-project)
- [📤 Exporting results](#-exporting-results)
- [🤖 Running in CI](#-running-in-ci)
- [🔧 Troubleshooting](#-troubleshooting)
- [💬 Getting help](#-getting-help)

## 📋 Requirements

GValidator runs in the Unity Editor and Unity batch mode. See [ProjectVersion.txt](UnityProject/ProjectSettings/ProjectVersion.txt) for the Unity version used for development and testing. Other Unity versions have not yet been verified. You do not need to enter Play mode to run validation.

## 📦 Installation

Install GValidator into **your existing Unity project** using one of the methods below. You do not need to clone this repository or open its example project.

### OpenUPM through Unity (recommended)

1. Open your project in Unity.
2. Go to **Edit → Project Settings → Package Manager**.
3. Under **Scoped Registries**, add the following registry and click **Apply**:

   | Field | Value |
   | --- | --- |
   | Name | `OpenUPM` |
   | URL | `https://package.openupm.com` |
   | Scope(s) | `com.guillemsc` |

   If OpenUPM is already listed, add `com.guillemsc` to its scopes.
4. Open **Window → Package Management → Package Manager** (or **Window → Package Manager**, depending on your Unity version).
5. Select **+ → Install package by name** (called **Add package by name** in some versions).
6. Enter `com.guillemsc.gvalidator` as the name. Leave the version empty to install the latest published release, then click **Install**.
7. Wait for Unity to finish compiling, then open **Window → GValidator → Validation Window**.

See the [OpenUPM package page](https://openupm.com/packages/com.guillemsc.gvalidator/) for published versions.

<details>
<summary>Already using the OpenUPM CLI?</summary>

Run this command from your Unity project root (the folder containing `Assets`, `Packages`, and `ProjectSettings`):

```sh
openupm add com.guillemsc.gvalidator
```

</details>

### Git URL

Git must be installed and available to Unity. Releases use branches named `release/x.x.x`.

1. Find the release branch you want to install in the repository's [branch list](https://github.com/Guillemsc/UnityValidator/branches).
2. Replace `x.x.x` in the URL below with the version from that branch's name.
3. Open the **Package Manager**, select **+ → Install package from git URL** (or **Add package from git URL**), and paste your completed URL:

```text
https://github.com/Guillemsc/UnityValidator.git?path=/UnityProject/Assets/GValidator#release/x.x.x
```

`x.x.x` is a placeholder, not a branch to install literally. To switch releases later, install using the URL for the new release branch. Use OpenUPM above if you prefer to select published updates through Package Manager.

### Release download

For releases that provide one, you can download the attached `.unitypackage` from [GitHub Releases](https://github.com/Guillemsc/UnityValidator/releases) and import it through **Assets → Import Package → Custom Package**.

OpenUPM or a Git dependency is preferable for most teams because Unity records the dependency in `Packages/manifest.json`. A `.unitypackage` copies files into `Assets/`, so updates and removals must be managed manually.

### Finish setup

Wait for Unity to finish importing and compiling, then open **Window → GValidator → Validation Window**. If the menu is missing, check the Unity Console for compilation errors.

For a Package Manager installation, commit `Packages/manifest.json` and `Packages/packages-lock.json` so teammates get the same dependency.

## 🚀 Quick start

### Run your first check

1. In Unity's **Project** window, right-click a prefab or a folder containing your assets.
2. Choose **Validate**. GValidator opens and runs the enabled checks on that selection.
3. Select a result row to read the full message and object path. Click its object to locate it in Unity.
4. Fix the issue, then click **Run Scoped** to check the same selection again.

After a completed run, an empty result list means there are no visible findings. If you expected results, check the severity filters, selected sources, and enabled validators. GValidator reports problems; you make the fixes in your project.

### Check the whole project

1. Open **Window → GValidator → Validation Window**.
2. Under **Sources**, enable **Assets** for assets and prefabs, and click **Select Scenes** to include all scenes.
3. Leave the built-in **Validators** enabled for your first run.
4. Click **Run All**. This checks the selected sources under `Assets/`, even if you previously ran a scoped check.

The result counters in the top-right show the number of info messages, warnings, and errors. Use the counters as toggles to show or hide each severity.

**Next:** [ignore generated or third-party folders](#-ignoring-folders), [share your team's settings](#-sharing-project-settings), or [add your own rules](#-adding-validation-to-your-project).

## 🖥 Using the validation window

### Run controls

| Control | Description |
| --- | --- |
| **Run All** | Validates all configured sources below `Assets/`. |
| **Run Scoped** | Reruns validation on the folder, asset, or object shown in the toolbar. |
| **Clear Scope** | Returns the scope to the complete `Assets/` folder. |
| **Clear** | Clears the current results. |
| **Global Configuration** | Selects or creates the project's GValidator configuration asset. |
| **Export XML** | Saves the current result as a formatted JUnit XML report. |

Validation displays a cancellable progress bar. Canceling a run keeps the previous completed result.

### Sources

The **Sources** section controls where validation gets its assets:

<img width="1078" height="388" alt="GVAlidator-Sources" src="https://github.com/user-attachments/assets/512ababf-e462-4c75-82d7-c60bfb7ac3f4" />

- **Assets** includes regular assets and prefabs found through the Asset Database.
- Each project scene is listed as a separate source.
- **Select Scenes** enables all scene sources.
- **Deselect Scenes** disables all scene sources.

Scenes are loaded for validation without modifying your current scene setup. Prefabs and scenes are traversed so rules can inspect their `GameObject`s and components.

### Validators

The **Validators** section controls which rules run. The **Assets** parent contains the built-in per-asset rules. Disable rules that are not relevant to your project or to the current investigation.

Disabling the **Assets** parent skips all of its children. Individual child selections remain available when the parent is enabled again.

Changes made in the validation window are local overrides for the current developer and project. An **Overridden** label marks a rule that differs from the shared project configuration. Click **Clear** beside one rule, or **Clear Overrides** above the list, to return to the project defaults.

To change the defaults used by the whole team and CI, click **Global Configuration** and edit **Enabled Validators** on the settings asset instead.

### Results

Each result includes:

- Severity: info, warning, or error.
- Validation message.
- Affected Unity object.
- Validator name.
- Asset path or scene hierarchy path.

Select a row to see its full details. The detail message, validator name, and object path are selectable, so they can be copied. Click an object in the results list to select and ping it in Unity.

## 🔍 Validating an asset or folder

You do not need to open the validation window first.

### From the Project window

1. Select an asset or folder.
2. Right-click it.
3. Choose **Validate**.

GValidator opens the validation window, sets the selected path as the scope, and runs validation.

### From a ScriptableObject Inspector

Open the Inspector context menu for a saved `ScriptableObject` and choose **Validate**.

### From a GameObject

Right-click a scene `GameObject` in the Hierarchy and choose **Validate**. GValidator validates that object and its attached components, without traversing its children. This direct object check uses the enabled validators but does not use the Sources selection or ignored folders. To validate a prefab hierarchy, right-click the prefab asset in the Project window instead.

### How scopes work

| Selected scope | What is validated |
| --- | --- |
| `Assets/` | The complete project asset tree. |
| A folder | Assets and selected scenes below that folder. |
| A file | Only that asset. |

The current scope is displayed in the top toolbar. Folder and asset runs still respect selected sources, enabled validators, and ignored folders. When validating a scene asset, enable that scene in **Sources**.

## 🤝 Sharing project settings

GValidator creates a `GValidatorProjectSettings` asset when first needed, including when you open the validation window. Click **Global Configuration** to select it in the Inspector. Its default location is:

```text
Assets/GValidatorProjectSettings.asset
```

The asset stores the team's:

- Ignored folders.
- Enabled and disabled validators.
- Suppressed findings and their reasons.

Commit both `GValidatorProjectSettings.asset` and its `.meta` file to version control. The Editor window and CI then use the same baseline. Validator choices made directly in the validation window remain local `EditorPrefs` overrides and are not committed; use the settings asset when changing team defaults.

| Setting | Where to change it | Shared with the team and CI? |
| --- | --- | --- |
| Enabled validators | **Global Configuration → Enabled Validators** | Yes |
| Ignored folders and suppressed findings | **Global Configuration** or a result's **Suppress Finding…** action | Yes |
| Local validator overrides | **Validators** in the validation window | No |
| Selected sources and current scope | Validation window | No; CI checks all assets and scenes under `Assets/` |

For a team rollout, run the built-in checks first, exclude generated or third-party content, and fix findings or suppress accepted exceptions. Commit the settings, then [add CI validation](#-running-in-ci) once you have a passing baseline.

## 📁 Ignoring folders

Use ignored folders for generated content, imported packages, third-party assets, or any path that should never be validated.

1. Open the validation window.
2. Click **Global Configuration**.
3. Add project-relative paths to **Ignored Folders** in the Inspector.

Examples:

```text
Assets/ThirdParty
Assets/Generated
Assets/AddressablesContent
```

Ignored folders apply to full-project, folder, asset, and scene runs, including CI. They exclude content from validation, so it produces no findings in those reports. After changing ignored folders, run validation again to refresh the results and XML export. Direct **Hierarchy → Validate** checks inspect the selected object regardless of ignored folders.

The folder and every descendant are excluded. Matching is case-insensitive and accepts `/` or `\` separators. Matching observes folder boundaries, so ignoring `Assets/Generated` does not ignore `Assets/GeneratedBackup`.

## 🔕 Suppressing individual findings

Select a result in the validation window and click **Suppress Finding…** in its details. Enter a reason and click **Suppress**. Suppressions are saved to the project settings asset; commit that asset to share them with the team and CI. Results without a stable object path cannot be suppressed.

The window hides suppressed findings by default. Toggle **Show Suppressed** to inspect them and include them in the window's severity counters; read their reasons and select **Remove Suppression** in the details panel. The project settings Inspector also lists suppressions, so you can remove stale entries there. Suppression changes take effect immediately in the current window without rerunning validation.

A suppression matches the validator name, severity, exact message text, object path, and Unity object ID when available. If any of these change, the new finding is reported normally. Suppressed findings are excluded from XML exports, CI reports, and the CI failure decision, regardless of the window's **Show Suppressed** setting. They do not disable the validator or skip its execution.

## ✅ Built-in validation rules

These checks run automatically on matching objects when their validators are enabled:

| Rule | What it checks |
| --- | --- |
| **Missing Scripts** | Missing `MonoBehaviour` scripts on prefab and scene `GameObject`s. |
| **GameObject Layer** | Invalid layer indices or unnamed layer slots. |
| **Shader Compilation** | Shader compiler errors, with source file and line information when available. |
| **Material Shader** | Materials with no shader or a shader with compilation errors. |
| **Invalid Unity Events** | Persistent listeners with missing targets, no selected method, or missing/incompatible methods. Disabled listeners are ignored. |

Three more checks run when you opt in through your own scripts:

| Rule | How to use it |
| --- | --- |
| **Not Null** | Add `[NotNull]` to a serialized reference that must be assigned. |
| **String Not Empty** | Add `[StringNotEmpty]` to a serialized string that must contain a value. |
| **IValidable** | Implement `IValidable` on a component or `ScriptableObject` to report your own findings. |

## 🛠 Adding validation to your project

Start with the simplest option that fits your rule:

| What you want to check | Use |
| --- | --- |
| A reference must be assigned or a string must have a value | [Field attributes](#field-attributes) |
| A rule belongs to one of your components or ScriptableObjects | [`IValidable`](#option-1-implement-ivalidable) |
| A reusable rule inspects matching objects without changing their classes | [`IAssetValidator`](#option-2-create-a-reusable-asset-validator) |
| A reusable rule targets one ScriptableObject type | [`ScriptableObjectValidator<T>`](#option-3-validate-one-scriptableobject-type) |
| A rule needs complete control over discovery and execution | [`IValidator`](#option-4-create-a-validator-from-scratch) |

### Assembly definition references

If your scripts use Unity's default assemblies, no assembly reference setup is needed. Put Editor-only validators in a folder such as `Assets/Editor/Validators`.

If your project uses custom assembly definitions (`.asmdef` files), select the relevant `.asmdef` in Unity, add these **Assembly Definition References** in the Inspector, and click **Apply**:

| Your code | Assembly references |
| --- | --- |
| Attributes and `IValidable` | `GValidator.Runtime` |
| `IAssetValidator`, `IValidator`, and Editor-only validator base classes | `GValidator.Editor` and `GValidator.Runtime`, plus your assembly containing the types being validated |

For custom validators, use an Editor-only assembly: disable **Any Platform** and include only **Editor**. An `Editor` subfolder inside a custom runtime assembly does not by itself make those scripts Editor-only; give it its own Editor-only `.asmdef`.

### Field attributes

Add `[NotNull]` or `[StringNotEmpty]` to Unity-serialized fields. For example, save this as `Assets/Scripts/CharacterSettings.cs`:

```csharp
using GValidator.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Settings")]
public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, NotNull]
    GameObject _characterPrefab;

    [SerializeField, StringNotEmpty]
    string _displayName;

    [SerializeField]
    int _startingHealth = 100;

    public int StartingHealth => _startingHealth;
}
```

Create an asset with **Assets → Create → Game → Character Settings**, leave the prefab and display name empty, then right-click the asset and choose **Validate**. You should see findings for both fields. Assign a prefab and enter a display name, then rerun validation to clear them.

`NotNull` also marks null fields in the Inspector with an error icon and red **Not Null** text. It supports Unity object references, exposed references, managed references, nested serialized fields, and collection elements.

The `StartingHealth` property is used by the custom-validator examples below; field attributes alone do not check its value.

### Option 1: implement `IValidable`

Use `IValidable` when a rule belongs to one of your own components or `ScriptableObject` types. Save this example as `Assets/Scripts/SpawnPoint.cs`:

```csharp
using GValidator.Validation.Builder;
using GValidator.Validation.Validables;
using UnityEngine;

public sealed class SpawnPoint : MonoBehaviour, IValidable
{
    [SerializeField]
    Transform _spawnTransform;

    [SerializeField]
    float _radius;

    public void Validate(IValidationBuilder validation)
    {
        if (_spawnTransform == null)
        {
            validation.Error("Spawn Transform is not assigned");
        }

        if (_radius <= 0f)
        {
            validation.Warning("Spawn radius should be greater than zero");
        }
    }
}
```

Available severities are:

```csharp
validation.Error("This must be fixed");
validation.Warning("This should be reviewed");
validation.Info("Useful information");
```

GValidator associates each message with the object and includes its asset or scene hierarchy path in the result.

Attach `SpawnPoint` to a prefab or scene object and include it in a validation run. Leave **IValidable** enabled under **Validators → Assets**. The example reports an error for an unassigned transform and a warning when the radius is zero or negative.

`IValidable` is part of the runtime assembly, so your component does not need to reference an Editor assembly. GValidator calls it during validation; implementing it does not schedule checks during gameplay.

### Option 2: create a reusable asset validator

Use `IAssetValidator` for a rule that should inspect every matching asset or object in the selected sources. This example uses `CharacterSettings` from [Field attributes](#field-attributes). Save it as `Assets/Editor/Validators/PositiveStartingHealthValidator.cs` (or in your custom Editor-only assembly):

```csharp
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEngine;
using Object = UnityEngine.Object;

[Validator("Positive Starting Health")]
public sealed class PositiveStartingHealthValidator : IAssetValidator
{
    public bool CanValidate(Object asset)
    {
        return asset is CharacterSettings;
    }

    public Task ValidateAsync(
        Object asset,
        IValidationBuilder validation,
        IValidationContext context)
    {
        CharacterSettings settings = (CharacterSettings)asset;

        if (settings.StartingHealth <= 0)
        {
            validation.Error("Starting Health must be greater than zero");
        }

        return Task.CompletedTask;
    }
}
```

GValidator discovers concrete `IAssetValidator` classes automatically. A custom validator must:

- Be in an Editor-only assembly.
- Be concrete and non-generic.
- Have a parameterless constructor.
- Return `true` from `CanValidate` for the objects it supports.

The `[Validator]` name is shown in the validator list and results. If the attribute is omitted, GValidator uses the class name.

After Unity compiles, reopen the validation window and look for **Positive Starting Health** under **Validators → Assets**. Set a `CharacterSettings` asset's starting health to `0` and validate it to try the rule.

GValidator supplies regular assets, `ScriptableObject`s, prefab and scene `GameObject`s, and their components to matching validators. Your validator should inspect the supplied object instead of searching the entire Asset Database itself.

For a long-running loop, yield periodically so the Editor stays responsive:

```csharp
await context.FrameSlicer.TrySlice();
```

### Option 3: validate one ScriptableObject type

For a synchronous rule dedicated to one `ScriptableObject` type, derive from `ScriptableObjectValidator<T>`. This is a shorter alternative to Option 2. Use the same `CharacterSettings` class and save this as `Assets/Editor/Validators/CharacterSettingsValidator.cs`:

```csharp
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validators.ScriptableObjects;

[Validator("Character Settings")]
public sealed class CharacterSettingsValidator
    : ScriptableObjectValidator<CharacterSettings>
{
    protected override void Validate(
        CharacterSettings asset,
        IValidationBuilder validation)
    {
        if (asset.StartingHealth <= 0)
        {
            validation.Error("Starting Health must be greater than zero");
        }
    }
}
```

The base class handles type filtering and passes only matching assets to your validation method. Choose either this example or Option 2 for starting-health validation; enabling both reports the same problem twice.

### Option 4: create a validator from scratch

<details>
<summary>Advanced: custom discovery, iteration, and progress</summary>

Implement `IValidator` when you need complete control over discovery, iteration, progress, and reporting. A standalone validator appears as a top-level entry in the Validators list instead of as a child of **Assets**.

The following example finds textures through GValidator's configured asset provider and reports textures larger than a project-defined limit:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using UnityEngine;
using Object = UnityEngine.Object;

[Validator("Texture Size")]
public sealed class TextureSizeValidator : IValidator
{
    const int MaximumSize = 4096;

    public async Task ValidateAsync(
        IValidationBuilder validation,
        IValidationContext context,
        IProgressScope progress)
    {
        List<Object> assets = await context.AssetsProvider.GetAssetsAsync("t:Texture2D");

        for (int index = 0; index < assets.Count; index++)
        {
            Object asset = assets[index];
            IProgressScope assetProgress = progress.Step(index, assets.Count, asset.name);
            assetProgress.Report(0f);

            if (asset is Texture2D texture)
            {
                validation.SetObject(texture);
                try
                {
                    bool isTooLarge = texture.width > MaximumSize
                                      || texture.height > MaximumSize;
                    if (isTooLarge)
                    {
                        validation.Error(
                            $"Texture is {texture.width}x{texture.height}; " +
                            $"the maximum is {MaximumSize}x{MaximumSize}");
                    }
                }
                finally
                {
                    validation.ClearObject();
                }
            }

            assetProgress.Report(1f);
            await context.FrameSlicer.TrySlice();
        }

        progress.Report(1f);
    }
}
```

A standalone validator can use:

- `context.AssetsProvider` to retrieve assets while respecting selected sources, the current scope, and ignored folders.
- `validation.SetObject(...)` before reporting a message so results include the affected object and its path.
- `validation.ClearObject()` after validating that object. Use `finally` so the context is cleared if validation throws.
- `progress.Step(...)` and `progress.Report(...)` to display meaningful progress.
- `context.FrameSlicer.TrySlice()` during long operations to keep the Editor responsive.

GValidator sets the validator name before calling `ValidateAsync`, so a standalone validator normally does not need to call `SetValidatorName` itself.

As with an `IAssetValidator`, put the class in an Editor-only assembly, make it concrete and non-generic, and provide a parameterless constructor so GValidator can discover it.

</details>

## 📤 Exporting results

After validation completes, click **Export XML** in the bottom-right toolbar.

The export uses formatted JUnit XML and contains:

- Total validation message count.
- Error count as failed tests.
- Warning count as skipped tests.
- One testcase per validation message.
- Validator name.
- Message text.
- Asset or hierarchy path.

The Editor export and CI report use the same format, so a report can be opened by tools that support JUnit results.

## 🤖 Running in CI

GValidator exposes this Unity batch-mode entry point:

```text
GValidator.Ci.GValidatorCi.Run
```

It performs the following steps:

1. Loads the global configuration.
2. Logs the ignored folders.
3. Checks all assets and scenes under `Assets/` with the validators enabled in the project settings.
4. Writes `gvalidator-results.xml`.
5. Exits with code `1` if unsuppressed errors were found, or `0` if validation passed.

Warnings are included in the report but do not fail CI. Unexpected exceptions also produce exit code `1`.

**CI uses the shared project settings, not your window selections.** Local validator overrides, selected scenes, and the current scope do not limit CI runs. To reproduce CI's coverage in the Editor, click **Clear Overrides**, enable **Assets**, click **Select Scenes**, and choose **Run All**.

### Run locally in batch mode

```sh
Unity \
  -batchmode \
  -quit \
  -projectPath "/path/to/your/UnityProject" \
  -executeMethod GValidator.Ci.GValidatorCi.Run \
  -logFile gvalidator.log
```

Run this from your Unity project root, and replace the project path and `Unity` with your actual paths. Quote executable paths containing spaces. This example uses Bash line continuations; in PowerShell or Command Prompt, put the command on one line.

Use an installed, activated Unity Editor matching your project's version. Close the interactive Editor for that project before starting batch mode. After the run, inspect `gvalidator.log` and `gvalidator-results.xml` in the project directory.

### GitHub Actions example

1. Install GValidator in your project and commit the package dependency and project settings asset.
2. Configure your repository's **Settings → Secrets and variables → Actions** with the license secrets required by [GameCI's activation instructions](https://game.ci/docs/github/activation). Follow the instructions for your license type (`UNITY_LICENSE`, or the applicable `UNITY_EMAIL`, `UNITY_PASSWORD`, and `UNITY_SERIAL` credentials).
3. Create `.github/workflows/gvalidator.yml` in **your repository** with the following content.
4. Set `UNITY_PROJECT_PATH` to `.` if `Assets`, `Packages`, and `ProjectSettings` are at your repository root, or to the relative project folder, such as `UnityProject`.

```yaml
name: GValidator CI

on:
  push:
  pull_request:
  workflow_dispatch:

permissions:
  contents: read
  checks: write

env:
  UNITY_PROJECT_PATH: .

jobs:
  gvalidator:
    runs-on: ubuntu-latest
    steps:
      - name: Check out your project
        uses: actions/checkout@v4

      - name: Run GValidator
        uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
          UNITY_SERIAL: ${{ secrets.UNITY_SERIAL }}
        with:
          projectPath: ${{ env.UNITY_PROJECT_PATH }}
          testMode: EditMode
          customParameters: -executeMethod GValidator.Ci.GValidatorCi.Run
          githubToken: ${{ secrets.GITHUB_TOKEN }}

      - name: Upload validation report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: gvalidator-results
          path: ${{ env.UNITY_PROJECT_PATH }}/gvalidator-results.xml
          if-no-files-found: warn
```

Commit and push the workflow. Open your repository's **Actions** tab, select **GValidator CI**, and inspect the run. The `gvalidator-results` artifact contains the XML report, including when validation findings fail the job.

The [workflow in this repository](.github/workflows/gvalidator.yml) also shows how to publish the JUnit report as a GitHub check. It is a standalone example, not a reusable workflow callable with a job-level `uses`.

### Other CI providers

Use the [batch-mode command above](#run-locally-in-batch-mode) in any runner that has Unity installed and activated. Preserve `gvalidator-results.xml` as a JUnit report artifact. The Unity process exits with code `1` when validation errors are found, so no extra result parsing is required.

## 🔧 Troubleshooting

### Package Manager cannot find GValidator

Check the scoped registry URL (`https://package.openupm.com`) and scope (`com.guillemsc`), then click **Apply**. Use the exact package name `com.guillemsc.gvalidator`. For a Git installation, check that Git is available to Unity, include the complete package path from [Git URL](#git-url), and replace `x.x.x` with an existing release branch's version.

### My installed version does not have a feature shown here

Check the [release notes](https://github.com/Guillemsc/UnityValidator/releases) for the feature's availability and read the README on the branch matching your installed release. For OpenUPM installations, check Package Manager for available updates. For Git installations, update the URL to the appropriate `release/x.x.x` branch as described in [Git URL](#git-url).

### The GValidator menu is missing

Wait for Unity to finish importing and compiling the package. Then check the Console for compilation errors. The window is available at **Window → GValidator → Validation Window**.

### My scripts cannot resolve GValidator types

If the scripts are compiled through a custom `.asmdef`, add `GValidator.Runtime` to its assembly references. Editor-only custom validators also need `GValidator.Editor` and a reference to the assembly defining your own validated types. See [Assembly definition references](#assembly-definition-references).

### No assets are validated

Check that:

- **Assets** is enabled in Sources.
- At least one validator is enabled.
- The current scope contains matching assets.
- The folder is not listed in **Ignored Folders**.

### Scenes are not validated

Select the scene in Sources or click **Select Scenes**. A folder scope only includes scenes below that folder.

### A field attribute does not report an error

Both `[NotNull]` and `[StringNotEmpty]` operate on Unity-serialized fields. Confirm that the field is public or has `[SerializeField]`, and that its containing object is part of a selected asset, prefab, or scene source.

### A custom validator is not shown

Confirm that the class:

- Implements `IAssetValidator` or `IValidator`, or derives from a supported validator base.
- Is in an Editor-only assembly.
- References `GValidator.Editor` and `GValidator.Runtime` from that assembly.
- Is concrete and non-generic.
- Has a parameterless constructor.
- Compiles without errors.

### A custom validator is shown but never runs

Check its `CanValidate` implementation. It must return `true` for the exact object type supplied by GValidator. For prefabs and scenes, GValidator supplies both `GameObject`s and their individual components.

### An ignored folder is still validated

Use a project-relative path beginning with `Assets/`, such as `Assets/Generated`. Start a new validation run after editing the configuration. Ensure the configured path identifies a folder rather than an individual asset.

### CI does not produce a report

Check that:

- The execute method is exactly `GValidator.Ci.GValidatorCi.Run`.
- Unity finishes compiling before running the method.
- The process can write to the project working directory.
- The Unity log does not contain an earlier compilation or licensing failure.

### CI passes when warnings exist

This is expected. Only errors produce exit code `1`. Warnings are represented as skipped testcases in the JUnit report.

### CI reports issues that I do not see locally

CI checks all scenes and assets under `Assets/` using the committed project settings. In the window, clear local validator overrides, enable **Assets** and all scenes, and use **Run All**. Check that the latest settings asset and its `.meta` file are committed, and that the relevant severity filters are visible.

## 💬 Getting help

If you are stuck or find a bug, [open an issue](https://github.com/Guillemsc/UnityValidator/issues). Include your Unity version, GValidator version or Git revision, installation method, steps to reproduce, and any relevant Console or CI log messages.

Contributions are welcome! See the [contributing guide](CONTRIBUTING.md) for choosing a target release, setting up the project, and opening a pull request.

## License

GValidator is available under the [MIT License](UnityProject/LICENSE).
