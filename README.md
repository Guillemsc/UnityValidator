[![openupm](https://img.shields.io/npm/v/com.guillemsc.gvalidator?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.guillemsc.gvalidator)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![Tests](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml/badge.svg)](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml)

# GValidator

GValidator is an extensible asset and scene validation framework for Unity. It finds project problems before they reach a build, a release, or another developer's machine.

It provides:

- A UI Toolkit validation window inside the Unity Editor.
- Validation of assets, prefabs, scenes, `GameObject`s, components, and `ScriptableObject`s.
- Built-in validators for common Unity project problems.
- Attribute-based validation for serialized fields.
- Runtime-safe `IValidable` checks for project-specific rules.
- Reflection-based discovery of custom Editor validators.
- Folder, asset, and project-wide validation scopes.
- Globally ignored folders shared by the Editor and CI.
- Progress reporting and frame slicing for long-running validation.
- JUnit XML output for CI systems and editor export.
- Unity batch-mode integration with a failure exit code.

## Requirements

The repository is developed and tested with Unity `6000.3.23f1`. The package uses Unity Editor APIs for asset discovery and validation, so validation itself runs in the Unity Editor or in Unity batch mode.

The package is distributed under the [MIT License](LICENSE).

## Installation

### Install from Git URL

In Unity, open **Window → Package Manager**, select **+ → Install package from git URL**, and enter a release tag:

```text
https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#YOUR_RELEASE_TAG
```

Replace `YOUR_RELEASE_TAG` with the release tag you want to install, for example `vX.Y.Z`.

You can also add the package directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.guillemsc.gvalidator": "https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#YOUR_RELEASE_TAG"
  }
}
```

### Install from OpenUPM

After the package is available in the OpenUPM registry, install it with the [OpenUPM CLI](https://openupm.com/docs/getting-started-cli.html):

```sh
openupm add com.guillemsc.gvalidator
```

Or configure the scoped registry and package in `Packages/manifest.json`:

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
    "com.guillemsc.gvalidator": "YOUR_PACKAGE_VERSION"
  }
}
```

## Quick start

1. Install GValidator.
2. Open **Window → GValidator → Validation Window**.
3. Select the asset sources and validators to run.
4. Click **Run All**.
5. Select a result to inspect its message, validator, and object path.
6. Use **Export XML** in the bottom-right toolbar to save a JUnit report.

You can also validate directly from the Project window:

- Select an asset or folder, right-click, and choose **Validate**.
- Select a `ScriptableObject` in the Inspector and use the context menu's **Validate** command.

The selected asset or folder becomes the current validation scope. Use **Run Scoped** to rerun that scope, or **Clear Scope** to return to the complete `Assets/` folder.

## Editor validation window

The validation window contains four main areas.

### Toolbar

- **Run All** validates the complete `Assets/` folder.
- **Clear** removes the current results.
- **Global Configuration** selects or creates the global `GValidatorConfiguration` asset.
- **Scope** displays the current folder or asset scope.
- **Run Scoped** runs the current non-global scope.
- **Clear Scope** resets the scope to `Assets/`.
- Severity toggles show or hide info, warning, and error messages.

### Sources

The default sources are:

- **Assets** — assets discovered below the selected search folders.
- **Scenes** — scene assets discovered in the project. Each scene can be selected independently.

Use **Select Scenes** or **Deselect Scenes** to change all scene selections at once. Source selection affects the Editor window. CI uses the default source set built by `ValidationDefinitionBuilder`.

### Validators

Validators are displayed as a hierarchy. The built-in **Assets** validator owns the individual asset checks. You can enable or disable a parent or child validator independently according to the hierarchy's selection rules.

### Results and details

Results can be filtered by severity. Selecting a result displays:

- The validation message.
- The validator that produced it.
- The asset or scene/hierarchy path associated with the object.

The detail text can be selected and copied. The **Export XML** button writes the current result in the same JUnit XML format used by CI.

## Validation scopes

GValidator supports three scopes:

| Scope | Example | Behavior |
| --- | --- | --- |
| All assets | `Assets/` | Searches the complete project asset tree. |
| Folder | `Assets/Characters/` | Searches assets below the selected folder. |
| Asset | `Assets/Characters/Hero.prefab` | Searches the containing folder, then keeps only the selected asset. |

Paths are normalized to use `/`. A selected asset is still filtered by its exact path after the source query completes.

Ignored folders are applied after source discovery, so they always win over the current scope and source selection.

## Global configuration

GValidator uses a `GValidatorConfiguration` `ScriptableObject` for project-wide settings. The default asset path is:

```text
Assets/GValidatorConfiguration.asset
```

Open it from the validation window with **Global Configuration**. Unity's Inspector can then edit the serialized settings.

### Ignored folders

Add project-relative paths to **Ignored Folders**, for example:

```text
Assets/ThirdParty
Assets/Generated
Assets/AddressablesContent
```

An ignored folder excludes the folder itself and all descendants. Matching is case-insensitive, and both `/` and `\\` separators are accepted. The filtering is boundary-aware, so `Assets/Generated` does not exclude `Assets/GeneratedBackup`.

Ignored folders apply to:

- Editor validation from the main window.
- Validation started from a selected folder or asset.
- CI validation.
- Scene assets as well as regular assets.

CI logs the effective ignored-folder list before running validation.

## Built-in validators

### Assets

The **Assets** validator is the parent traversal pipeline. It retrieves assets from the selected sources, then validates each asset once with the enabled child validators.

For prefabs and scene roots it traverses the hierarchy and validates:

- The `GameObject`.
- Every component on each `GameObject`.

For regular assets and `ScriptableObject`s it validates the loaded main asset directly.

### Not Null

The **Not Null** validator checks serialized fields marked with `[NotNull]`:

```csharp
using GValidator.NotNulls.Attributes;
using UnityEngine;

public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, NotNull]
    GameObject _prefab;
}
```

It supports object references, exposed references, and managed references.

### String Not Empty

The **String Not Empty** validator checks serialized string fields marked with `[StringNotEmpty]`:

```csharp
using GValidator.Strings.Attributes;
using UnityEngine;

public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, StringNotEmpty]
    string _displayName;
}
```

### Missing Scripts

The **Missing Scripts** validator reports `GameObject`s with missing `MonoBehaviour` scripts. It applies to prefab and scene hierarchies.

### GameObject Layer

The **GameObject Layer** validator checks that every `GameObject` uses a valid, named Unity layer.

### Shader Compilation

The **Shader Compilation** validator reports shader compiler errors and includes the source file and line when Unity provides that information.

### Material Shader

The **Material Shader** validator reports materials without an assigned shader and materials whose shader has compilation errors.

### IValidable

The **IValidable** validator discovers `IValidable` implementations on supported objects and calls their `Validate` method.

### Invalid Unity Events

The **Invalid Unity Events** validator checks serialized persistent UnityEvent listeners for:

- Missing listener targets.
- Missing method names.
- Listener methods that cannot be resolved with the configured argument mode.

## Add custom validation

There are three recommended extension styles. Choose the one that matches where your rule belongs.

### 1. Validate a runtime object with `IValidable`

Use `IValidable` when the rule belongs to a specific `MonoBehaviour` or `ScriptableObject` and should be available from runtime code.

```csharp
using GValidator.Validation.Builder;
using GValidator.Validation.Validables;
using UnityEngine;

public sealed class SpawnPoint : MonoBehaviour, IValidable
{
    [SerializeField]
    Transform _spawnTransform;

    public void Validate(IValidationBuilder validation)
    {
        if (_spawnTransform == null)
        {
            validation.Error("Spawn Transform is not assigned");
        }
    }
}
```

Report messages with:

```csharp
validation.Error("Something is invalid");
validation.Warning("Something should be reviewed");
validation.Info("Informational message");
```

The validation builder automatically associates the message with the current validator and object.

### 2. Add a discovered asset validator

Implement `IAssetValidator` in an Editor-only assembly when the validator should receive assets from the central Assets traversal.

```csharp
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEngine;
using Object = UnityEngine.Object;

[Validator("Positive Health")]
public sealed class PositiveHealthValidator : IAssetValidator
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
            validation.Error("Starting Health must be positive");
        }

        return Task.CompletedTask;
    }
}
```

Requirements:

- Implement `IAssetValidator`.
- Add `[Validator("Name shown in the window")]`.
- Provide a parameterless constructor. Reflection creates validator instances automatically.
- Put the implementation in an Editor-only assembly, because `IAssetValidator` and its context use Unity Editor APIs.
- Return `Task.CompletedTask` for synchronous checks.
- Use `await context.FrameSlicer.TrySlice()` inside long loops.

The central dispatcher handles asset sources, ignored folders, validation scopes, object context, progress, and frame slicing. An asset validator should not search the `AssetDatabase` or open scenes itself unless it has a specific reason to do so.

### 3. Reuse validator base classes

#### `ScriptableObjectValidator<T>`

For a check dedicated to one `ScriptableObject` type, inherit from `ScriptableObjectValidator<T>`:

```csharp
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validators.ScriptableObjects;

[Validator("Character Settings")]
public sealed class CharacterSettingsValidator : ScriptableObjectValidator<CharacterSettings>
{
    protected override void Validate(
        CharacterSettings asset,
        IValidationBuilder validation)
    {
        if (asset.StartingHealth <= 0)
        {
            validation.Error("Starting Health must be positive");
        }
    }
}
```

#### `SerializedFieldValidator<TAttribute>`

For an attribute-driven serialized-field check, inherit from `SerializedFieldValidator<TAttribute>` and implement `GetErrorMessage(SerializedProperty)`:

```csharp
using GValidator.Strings.Attributes;
using GValidator.Validation.Attributes;
using GValidator.Validators.SerializedFields;
using UnityEditor;

[Validator("String Not Empty")]
public sealed class StringNotEmptyValidator : SerializedFieldValidator<StringNotEmptyAttribute>
{
    protected override string? GetErrorMessage(SerializedProperty property)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            return null;
        }

        return string.IsNullOrEmpty(property.stringValue) ? "is empty" : null;
    }
}
```

### Group validators under a parent

Use `ValidatorWithChildren<TChild>` when a validator owns a family of child validators:

```csharp
using System.Linq;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;

[Validator("Project Rules")]
public sealed class ProjectRulesValidator : ValidatorWithChildren<IAssetValidator>
{
    public override async Task ValidateAsync(
        IValidationBuilder validation,
        IValidationContext context,
        IProgressScope progress)
    {
        foreach (IAssetValidator validator in EnabledChildren)
        {
            // Run custom child workflow here.
            await context.FrameSlicer.TrySlice();
        }
    }
}
```

The factory discovers concrete validator nodes by reflection. Child validators are displayed below their parent and are not displayed as separate root entries. `EnabledChildren` excludes children disabled by the selection provider.

Use `IValidator` directly for a standalone validator that manages its own workflow. It receives:

- `IValidationBuilder` for reporting messages.
- `IValidationContext` for sources, validators, and frame slicing.
- `IProgressScope` for progress reporting.

## Custom asset sources

An asset source implements `IAssetsSource`:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEngine;

public sealed class CustomAssetsSource : IAssetsSource
{
    public string Name => "Custom Assets";

    public async Task<List<Object>> GetAssetsAsync(
        string filter,
        string[] searchInFolders,
        IFrameSlicer frameSlicer)
    {
        List<Object> assets = new();

        // Discover and load assets here.
        await frameSlicer.TrySlice();

        return assets;
    }
}
```

An `IAssetsSource` receives:

- The source filter string.
- The folders for the current validation scope.
- The frame slicer.

The source should return loaded Unity objects. `AssetsProvider` applies the current scope and global ignored-folder filtering after all sources return their objects.

The built-in Editor window creates the default sources in `GValidatorWindow`. CI creates its default sources in `ValidationDefinitionBuilder`. To use a custom source in those flows, extend the corresponding composition code or create a custom `ValidationDefinition` and `ValidationContext`.

## Validation architecture

The main flow is:

```text
Asset sources
    ↓
AssetsProvider
    - validation scope filtering
    - ignored-folder filtering
    ↓
ValidationContext
    ↓
ValidationRunner
    ↓
IValidator / ValidatorWithChildren
    ↓
IValidationBuilder
    ↓
IValidationResult
    ↓
Editor window or JUnit XML report
```

Important types:

| Type | Assembly | Purpose |
| --- | --- | --- |
| `IValidable` | Runtime | Runtime-safe object validation hook. |
| `IValidationBuilder` | Runtime | Reports info, warning, and error messages. |
| `IValidationResult` | Runtime | Read-only validation result and counters. |
| `IValidator` | Editor | Standalone validation workflow. |
| `IAssetValidator` | Editor | Per-object asset validation. |
| `IAssetsSource` | Editor | Supplies assets to the pipeline. |
| `IValidationContext` | Editor | Provides validators, assets, and frame slicing. |
| `IFrameSlicer` | Editor | Yields between expensive work. |
| `IProgressScope` | Editor | Reports nested validation progress. |
| `ValidationReportWriter` | Editor | Converts results to JUnit XML. |

## Progress and frame slicing

Validation work is designed to keep the Unity Editor responsive:

- The Editor window uses `CancellableProgressBarProgressSink`.
- The progress bar can cancel validation.
- `FrameSlicer` yields with `Task.Yield()` when the frame budget is exceeded.
- CI uses a large frame budget and a no-op progress sink.

When implementing expensive custom checks, call:

```csharp
await context.FrameSlicer.TrySlice();
```

inside loops over serialized properties, objects, assets, or other large collections. Use the supplied `IProgressScope` when your validator has meaningful sub-steps:

```csharp
IProgressScope itemProgress = progress.Step(index, items.Count, item.name);
itemProgress.Report(0f);

// Validate item.

itemProgress.Report(1f);
```

## CI integration

The batch-mode entry point is:

```text
GValidator.Ci.GValidatorCi.Run
```

The command:

1. Loads or creates `Assets/GValidatorConfiguration.asset`.
2. Builds the default CI validation definition.
3. Applies configured ignored folders.
4. Logs ignored folders before validation.
5. Runs validation.
6. Generates JUnit XML.
7. Logs the XML and writes `gvalidator-results.xml`.
8. Exits with code `1` if at least one error exists; otherwise exits with code `0`.

Warnings do not fail CI by default. The current entry point treats only `ErrorCount > 0` as a failure.

### Run locally in batch mode

From a Unity installation, run:

```sh
Unity \
  -batchmode \
  -quit \
  -projectPath /path/to/project \
  -executeMethod GValidator.Ci.GValidatorCi.Run \
  -logFile gvalidator.log
```

Use the Unity executable name and path appropriate for your operating system. The report is written relative to the Unity project working directory unless the process is started with a different working directory.

### GitHub Actions

This repository includes `.github/workflows/gvalidator.yml`. It:

- Checks out the project.
- Runs Unity in EditMode batch mode through `game-ci/unity-test-runner`.
- Invokes `GValidator.Ci.GValidatorCi.Run`.
- Publishes `gvalidator-results.xml` as a JUnit check.
- Uploads the report as a workflow artifact.

Configure the Unity secrets required by GameCI:

- `UNITY_LICENSE`, or the email/password activation values.
- `UNITY_EMAIL` and `UNITY_PASSWORD` when using credential activation.
- `UNITY_SERIAL` when required by the selected Unity license.

### Use the CI runner API

The CI implementation is split into a definition and a configurable runner:

```csharp
ValidationDefinition definition = ValidationDefinitionBuilder.Build(
    NoOpProgressSink.Instance,
    ignoredFolders);

ValidationCiRunnerConfiguration runnerConfiguration = new(
    "validation-results.xml",
    Debug.Log);

ValidationCiRunner runner = new(definition, runnerConfiguration);
ValidationCiRunnerResult result = await runner.RunAsync();

// result.Xml contains the JUnit XML.
// result.ExitCode is 0 for success and 1 when errors were found.
```

`ValidationCiRunnerConfiguration` controls the report path and logging callback. `ValidationDefinition` owns the ignored folders because they are part of the validation definition, not the output mechanism.

## JUnit XML output

The editor **Export XML** button and CI use `ValidationReportWriter.ToJUnit` and therefore produce the same format.

The report contains:

- A `testsuite` named `GValidator`.
- The total message count in `tests`.
- Error count in `failures`.
- Warning count in `skipped`.
- One `testcase` per validation message.
- Validator name as the testcase class name.
- Message and object path in the testcase name.
- `<failure>` elements for errors.
- `<skipped>` elements for warnings.

The XML is formatted with indentation for readability.

## Testing

The repository contains EditMode tests for the validation runner, asset validation, and validator selection. Run them from Unity's Test Runner with the **EditMode** test mode.

The GitHub Actions workflow in `.github/workflows/test.yml` runs the EditMode tests using GameCI.

For a local project build check, the repository also contains generated project files. The Editor assembly can be built with:

```sh
dotnet build GValidator.Editor.csproj --no-restore
```

Unity remains the source of truth for importing assets, compiling assemblies, and running the actual Editor tests.

## Troubleshooting

### The validation window does not appear

Use **Window → GValidator → Validation Window**. If the menu is missing, allow Unity to finish importing and compiling the package, then check the Console for Editor assembly errors.

### A custom validator is not listed

Check that:

- The class implements `IValidator`, `IAssetValidator`, or derives from a supported validator base.
- The class is concrete and not generic.
- The class has a parameterless constructor.
- The class is in an Editor assembly for Editor-only interfaces.
- The class is not inside the test assembly.
- The assembly has compiled successfully.

Use `[Validator("Display Name")]` to control the name shown in the window. Without the attribute, the class name is used.

### A custom asset validator does not report anything

Verify that `CanValidate` returns `true` for the object type being traversed. Remember that `IAssetValidator` receives individual assets, prefab `GameObject`s, scene objects, and components depending on the source and traversal.

### An ignored folder is still being checked

Use a project-relative path beginning with `Assets/`, such as `Assets/Generated`. The path is matched against the asset path and all descendants. Reopen or select the `GValidatorConfiguration` asset after editing it, then start a new validation run.

### CI produces no report

Check that:

- Unity completed compilation before invoking the method.
- The `-executeMethod` value is exactly `GValidator.Ci.GValidatorCi.Run`.
- The project configuration asset can be loaded or created.
- The Unity process has permission to write `gvalidator-results.xml`.

### CI exits successfully when warnings exist

This is expected. CI fails only when the result contains one or more errors. Warnings are included in the JUnit report as skipped testcases but do not currently affect the exit code.

## Project structure

The package is organized into runtime and Editor assemblies:

```text
Assets/GValidator/
├── Scripts/
│   ├── Runtime/
│   │   ├── Configuration/
│   │   ├── NotNulls/
│   │   ├── Strings/
│   │   └── Validation/
│   │       ├── Builder/
│   │       ├── Messages/
│   │       ├── Objects/
│   │       ├── Result/
│   │       └── Validables/
│   ├── Editor/
│   │   ├── Ci/
│   │   ├── Validation/
│   │   │   ├── AssetSources/
│   │   │   ├── Assets/
│   │   │   ├── Ci/
│   │   │   ├── Context/
│   │   │   ├── FrameSlicing/
│   │   │   ├── Progress/
│   │   │   └── Validators/
│   │   ├── Validators/
│   │   └── Window/
│   └── Tests/
├── Uxml/
└── package.json
```

Keep runtime implementations free of Editor-only interfaces. Put Editor validators, asset sources, validation windows, CI entry points, and report writers in an Editor assembly.

## Contributing

1. Create a branch for the change.
2. Keep runtime and Editor dependencies in their respective assemblies.
3. Add or update EditMode tests for behavior changes.
4. Run the Editor assembly build and Unity tests locally.
5. Update this README when public APIs, workflows, or configuration change.
6. Open a pull request with the problem, implementation, and validation performed.
