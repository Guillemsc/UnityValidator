[![openupm](https://img.shields.io/npm/v/com.guillemsc.gvalidator?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.guillemsc.gvalidator)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![Tests](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml/badge.svg)](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml)

# GValidator for Unity

<img width="1640" height="856" alt="GValidator-Banner" src="https://github.com/user-attachments/assets/7ac37688-2e0d-46c2-861c-11062a5b9d0b" />

GValidator helps Unity teams find broken references, invalid assets, scene problems, and project-specific mistakes before they reach a build.

Run validation from a dedicated Editor window, validate one asset or folder from the Project window, or enforce the same rules in CI. Results identify the validator, affected object, and full asset or hierarchy path, and can be exported as XML.

**Package ID:** `com.guillemsc.gvalidator`

**Editor menu:** **Window → GValidator → Validation Window**

<img width="970" height="648" alt="image" src="https://github.com/user-attachments/assets/38663991-fa89-4626-89af-65f1ee80c4c3" />


## Features

- Validate the entire `Assets/` folder, one folder, or one asset.
- Validate regular assets, `ScriptableObject`s, prefabs, scenes, `GameObject`s, and components.
- Select which scenes and validation rules to run.
- Ignore generated, third-party, or other project folders globally.
- Suppress individual findings with a project-shared reason, including in CI.
- Filter results by info, warning, and error severity.
- Inspect and copy validation messages and object paths.
- Export results as formatted JUnit XML.
- Run the same validation in Unity batch mode and CI.
- Add validation directly to your own components with `IValidable`.
- Add reusable custom validators for project-wide rules.
- Keep the Editor responsive with cancellable progress and frame slicing.

## Contents

- [Installation](#installation)
- [Quick start](#quick-start)
- [Integrating GValidator into your project](#integrating-gvalidator-into-your-project)
- [Using the validation window](#using-the-validation-window)
- [Validating an asset or folder](#validating-an-asset-or-folder)
- [Sharing project settings](#sharing-project-settings)
- [Ignoring folders](#ignoring-folders)
- [Suppressing individual findings](#suppressing-individual-findings)
- [Built-in validation rules](#built-in-validation-rules)
- [Adding validation to your project](#adding-validation-to-your-project)
- [Exporting results](#exporting-results)
- [Running in CI](#running-in-ci)
- [Troubleshooting](#troubleshooting)

## Requirements

GValidator runs in the Unity Editor and Unity batch mode. The current package is tested with Unity `6000.3.23f1`. Other Unity versions have not yet been verified.

## Installation

### OpenUPM (recommended)

If you use the [OpenUPM CLI](https://openupm.com/docs/getting-started-cli.html), run this command from your Unity project directory:

```sh
openupm add com.guillemsc.gvalidator
```

To configure OpenUPM manually, add its scoped registry and the GValidator dependency to `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.guillemsc"]
    }
  ],
  "dependencies": {
    "com.guillemsc.gvalidator": "1.0.0"
  }
}
```

Replace `1.0.0` with the version you want to use. See the [OpenUPM package page](https://openupm.com/packages/com.guillemsc.gvalidator/) for published versions.

### Git URL

1. Open **Window → Package Manager** in Unity.
2. Select **+ → Install package from git URL**.
3. Enter a versioned package URL, replacing `RELEASE_TAG` with the version you want to install:

```text
https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#RELEASE_TAG
```

Always pin the URL to a release tag or commit so package updates are intentional. The package path changed after `v1.0.0`; for revisions using the repository's current layout, use:

```text
https://github.com/Guillemsc/UnityValidator.git?path=/UnityProject/Assets/GValidator#TAG_OR_COMMIT
```

You can also add the dependency to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.guillemsc.gvalidator": "https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#RELEASE_TAG"
  }
}
```

### Release download

For releases that provide one, you can download the attached `.unitypackage` from [GitHub Releases](https://github.com/Guillemsc/UnityValidator/releases) and import it through **Assets → Import Package → Custom Package**.

OpenUPM or a pinned Git dependency is preferable for most teams because Unity records the dependency in `Packages/manifest.json`. A `.unitypackage` copies files into `Assets/`, so updates and removals must be managed manually.

### Verify the installation

Wait for Unity to finish importing and compiling. Then open **Window → GValidator → Validation Window**. If the menu appears, the package is ready to use.

## Quick start

1. Open **Window → GValidator → Validation Window**.
2. Leave **Assets** enabled and select any scenes you want to inspect.
3. Leave all validators enabled for the first run.
4. Click **Run All**.
5. Select a finding to inspect its message, validator, object, and path.
6. Fix the issue or suppress it with a reason if it is an accepted exception.

The result counters in the top-right show the number of info messages, warnings, and errors. Use the counters as toggles to show or hide each severity.

For a faster trial, right-click a folder, prefab, or asset in the Project window and choose **Validate**. This opens GValidator and limits the run to your selection.

## Integrating GValidator into your project

A typical team setup is:

1. **Install and run the built-in rules.** Fix existing missing scripts, invalid UnityEvents, shader problems, and other findings.
2. **Exclude content you do not own.** Add generated and third-party directories to **Ignored Folders**.
3. **Choose project-wide validators.** Open **Global Configuration** and disable rules that should not run for your project.
4. **Add domain rules.** Use attributes or `IValidable` for rules tied to your own types; use a custom asset validator for reusable project-wide checks.
5. **Commit the settings asset.** This gives every developer and CI the same ignored folders, validator defaults, and suppressions.
6. **Add CI validation.** Run GValidator on pull requests once the project has a clean baseline.

### Assembly definition references

Scripts compiled into Unity's predefined assemblies need no extra setup. If your project uses custom assembly definitions (`.asmdef` files), add the appropriate references:

| Your code | Assembly to reference |
| --- | --- |
| Attributes and `IValidable` in runtime code | `GValidator.Runtime` |
| `IAssetValidator`, `IValidator`, and Editor-only validator base classes | `GValidator.Editor` and `GValidator.Runtime` |

Keep custom `IAssetValidator` and `IValidator` implementations in an Editor-only folder or assembly. They use Unity Editor APIs and must not be included in a player build.

## Using the validation window

### Run controls

| Control | Description |
| --- | --- |
| **Run All** | Validates all configured sources below `Assets/`. |
| **Run Scoped** | Validates only the currently selected folder or asset. |
| **Clear Scope** | Returns the scope to the complete `Assets/` folder. |
| **Clear** | Clears the current results. |
| **Global Configuration** | Selects or creates the project's GValidator configuration asset. |
| **Export XML** | Saves the current result as a formatted JUnit XML report. |

Validation displays a cancellable progress bar. Canceling a run keeps the previous completed result.

### Sources

The **Sources** section controls where validation gets its assets:

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

## Validating an asset or folder

You do not need to open the validation window first.

### From the Project window

1. Select an asset or folder.
2. Right-click it.
3. Choose **Validate**.

GValidator opens the validation window, sets the selected path as the scope, and runs validation.

### From a ScriptableObject Inspector

Open the Inspector context menu for a saved `ScriptableObject` and choose **Validate**.

### From a GameObject

Right-click a scene `GameObject` in the Hierarchy and choose **Validate**. GValidator validates that object and its attached components. To validate a prefab, right-click the prefab asset in the Project window instead.

### How scopes work

| Selected scope | What is validated |
| --- | --- |
| `Assets/` | The complete project asset tree. |
| A folder | Assets and selected scenes below that folder. |
| A file | Only that asset. |

The current scope is displayed in the top toolbar. A scoped run still respects selected sources, enabled validators, and ignored folders.

## Sharing project settings

Click **Global Configuration** in the validation window to select the project's `GValidatorProjectSettings` asset. If one does not exist, GValidator creates it at:

```text
Assets/GValidatorProjectSettings.asset
```

The asset stores the team's:

- Ignored folders.
- Enabled and disabled validators.
- Suppressed findings and their reasons.

Commit both `GValidatorProjectSettings.asset` and its `.meta` file to version control. The Editor window and CI then use the same baseline. Validator choices made directly in the validation window remain local `EditorPrefs` overrides and are not committed; use the settings asset when changing team defaults.

## Ignoring folders

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

Ignored folders apply everywhere:

- Full-project validation.
- Folder and asset scopes.
- Scene validation.
- Editor XML exports.
- CI validation.

The folder and every descendant are excluded. Matching is case-insensitive and accepts `/` or `\` separators. Matching observes folder boundaries, so ignoring `Assets/Generated` does not ignore `Assets/GeneratedBackup`.

## Suppressing individual findings

Select a result in the validation window and click **Suppress Finding…** in its details. Enter a reason and click **Suppress**. Suppressions are saved to the project settings asset; commit that asset to share them with the team and CI. Results without a stable object path cannot be suppressed.

The window hides suppressed findings by default. Toggle **Show Suppressed** to inspect them and include them in the window's severity counters; read their reasons and select **Remove Suppression** in the details panel. The project settings Inspector also lists suppressions, so you can remove stale entries there. Suppression changes take effect immediately in the current window without rerunning validation.

A suppression matches the validator name, severity, exact message text, object path, and Unity object ID when available. If any of these change, the new finding is reported normally. Suppressed findings are excluded from XML exports, CI reports, and the CI failure decision, regardless of the window's **Show Suppressed** setting. They do not disable the validator or skip its execution.

## Built-in validation rules

### Not Null

Marks serialized references that must be assigned. Null fields are reported during validation and are also identified in the Inspector with an error icon and red **Not Null** text.

```csharp
using GValidator.Attributes;
using UnityEngine;

public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, NotNull]
    GameObject _characterPrefab;
}
```

`NotNull` supports Unity object references, exposed references, and managed references. It can be used on nested serialized fields and collection elements.

### String Not Empty

Marks serialized strings that must contain a value:

```csharp
using GValidator.Attributes;
using UnityEngine;

public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, StringNotEmpty]
    string _displayName;
}
```

### Missing Scripts

Reports missing `MonoBehaviour` scripts on `GameObject`s in prefabs and scenes.

### GameObject Layer

Reports `GameObject`s that use an invalid layer index or an unnamed layer slot.

### Shader Compilation

Reports shader compiler errors. Source file and line information are included when Unity provides them.

### Material Shader

Reports materials that have no shader or use a shader with compilation errors.

### Invalid Unity Events

Checks persistent UnityEvent listeners for:

- Missing targets.
- No selected method.
- Methods that no longer exist or do not match the configured argument type.

Disabled persistent listeners are ignored.

### IValidable

Runs project-specific validation implemented directly by a `MonoBehaviour` or `ScriptableObject`. This is usually the simplest way to add domain rules to your own objects.

## Adding validation to your project

### Option 1: implement `IValidable`

Use `IValidable` when a rule belongs to one of your own components or `ScriptableObject` types.

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

`IValidable` is part of the runtime assembly, so your component does not need to reference an Editor assembly.

### Option 2: create a reusable asset validator

Use `IAssetValidator` for a rule that should inspect every matching asset or object in the project. Put the validator in an Editor-only assembly.

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

GValidator supplies regular assets, `ScriptableObject`s, prefab and scene `GameObject`s, and their components to matching validators. Your validator should inspect the supplied object instead of searching the entire Asset Database itself.

For a long-running loop, yield periodically so the Editor stays responsive:

```csharp
await context.FrameSlicer.TrySlice();
```

### Option 3: validate one ScriptableObject type

For a synchronous rule dedicated to one `ScriptableObject` type, derive from `ScriptableObjectValidator<T>`:

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

The base class handles type filtering and passes only matching assets to your validation method.

### Option 4: create a validator from scratch

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

### Choosing an extension method

| Requirement | Recommended option |
| --- | --- |
| A rule belongs to one component or ScriptableObject | `IValidable` |
| A serialized reference must be assigned | `[NotNull]` |
| A serialized string must have a value | `[StringNotEmpty]` |
| A reusable rule should inspect many object types | `IAssetValidator` |
| A reusable rule targets one ScriptableObject type | `ScriptableObjectValidator<T>` |
| A rule needs complete control over discovery and execution | `IValidator` |

## Exporting results

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

## Running in CI

GValidator exposes this Unity batch-mode entry point:

```text
GValidator.Ci.GValidatorCi.Run
```

It performs the following steps:

1. Loads the global configuration.
2. Logs the ignored folders.
3. Runs all default validation sources and enabled validators.
4. Writes `gvalidator-results.xml`.
5. Exits with code `1` if unsuppressed errors were found, or `0` if validation passed.

Warnings are included in the report but do not fail CI.

### Run locally in batch mode

```sh
Unity \
  -batchmode \
  -quit \
  -projectPath /path/to/project \
  -executeMethod GValidator.Ci.GValidatorCi.Run \
  -logFile gvalidator.log
```

Replace `Unity` with the path to the Unity executable on your operating system.

### GitHub Actions example

This repository provides a reusable workflow that runs validation, publishes the JUnit result as a GitHub check, and uploads the XML report. Add this job to a workflow in your project:

```yaml
name: GValidator CI

on:
  push:
  pull_request:

jobs:
  gvalidator:
    permissions:
      contents: read
      checks: write
    uses: Guillemsc/UnityValidator/.github/workflows/gvalidator.yml@main
    secrets: inherit
```

If the Unity project is not at the repository root, pass its path:

```yaml
    with:
      project-path: path/to/UnityProject
```

Configure the [Unity license secrets required by GameCI](https://game.ci/docs/github/activation). The workflow accepts either `UNITY_LICENSE`, or the Unity credential secrets `UNITY_EMAIL` and `UNITY_PASSWORD`; `UNITY_SERIAL` can also be passed when your license requires it. Pin the workflow reference to a release tag instead of `main` if you prefer reproducible CI configuration.

Commit `GValidatorProjectSettings.asset` so local and CI runs share the same ignored folders, disabled validators, and suppressions.

### Other CI providers

Use the [batch-mode command above](#run-locally-in-batch-mode) in any runner that has Unity installed and activated. Preserve `gvalidator-results.xml` as a JUnit report artifact. The Unity process exits with code `1` when validation errors are found, so no extra result parsing is required.

## Troubleshooting

### The GValidator menu is missing

Wait for Unity to finish importing and compiling the package. Then check the Console for compilation errors. The window is available at **Window → GValidator → Validation Window**.

### My scripts cannot resolve GValidator types

If the scripts are compiled through a custom `.asmdef`, add `GValidator.Runtime` to its assembly references. Editor-only custom validators also need `GValidator.Editor`. Scripts in Unity's predefined assemblies do not need explicit references.

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

## License

GValidator is available under the [MIT License](LICENSE).
