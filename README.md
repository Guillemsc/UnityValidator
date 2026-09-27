[![openupm](https://img.shields.io/npm/v/com.guillemsc.gvalidator?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.guillemsc.gvalidator)](https://openupm.com/packages/com.guillemsc.gvalidator/)
[![Tests](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml/badge.svg)](https://github.com/Guillemsc/UnityValidator/actions/workflows/test.yml)

# GValidator

GValidator is a generic Unity validation tool. It provides project-wide asset and scene validation, a selectable Editor window, progress reporting, and extension points for game-specific validation.

## Installation

### Install from Git URL

In Unity, open **Window → Package Manager**, select **+ → Install package from git URL**, and enter a release tag:

```text
https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#v1.0.0
```

Change `v1.0.0` to the version tag you want. You can also add the dependency to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.guillemsc.gvalidator": "https://github.com/Guillemsc/UnityValidator.git?path=/Assets/GValidator#v1.0.0"
  }
}
```

### Install from OpenUPM

After the package is listed on OpenUPM, install it with [OpenUPM CLI](https://openupm.com/docs/getting-started-cli.html):

```sh
openupm add com.guillemsc.gvalidator
```

Or add the OpenUPM scoped registry and package to `Packages/manifest.json`:

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

## Open the validation window

Open **Tools → GValidator → Validation Window**. The window lets you:

- Select which asset sources and validators to run.
- Run across all of `Assets/`, a selected folder, or a selected file.
- View messages by severity and inspect the validator and object path for the selected result.
- Use **Run Scoped** to rerun a folder/file scope or **Clear Scope** to reset the scope.

In Unity's Project window, right-click an asset or folder and choose **Validate** to open the window and validate that selection.

Validation is sliced across Editor frames when work takes longer than the configured frame budget. Progress is shown while validators run.

## Run validation in CI

The CI layer is independent of the Editor window. It creates the default asset sources and selected validators, runs validation, and returns a JUnit XML string in memory.

The batch-mode entry point is:

```text
GValidator.Ci.GValidatorCi.Run
```

The process logs the JUnit XML to Unity's output, writes it to `gvalidator-results.xml`, and exits with code `1` when errors are found and `0` when validation passes. Each report entry includes the failing object's asset and hierarchy path. Warnings do not fail CI by default; change `FailOnWarning` in `GValidatorCi` if warnings should fail the build.

For GitHub Actions, the repository includes `.github/workflows/gvalidator.yml`, which runs the CI validation and publishes the JUnit result as a check and an artifact. It requires the same Unity activation secrets as the Unity test runner.

## Built-in validators

- **Asset validation pipeline** — traverses selected assets, prefabs, and scenes once and dispatches each object to selected asset validators.
- **Not Null** — reports null serialized references on fields marked with `[NotNull]`.
- **String Not Empty** — reports empty serialized strings marked with `[StringNotEmpty]`.
- **Missing Scripts** — finds missing MonoBehaviour scripts on GameObjects in prefabs and scenes.
- **IValidable** — discovers and invokes `IValidable` implementations on ScriptableObjects and MonoBehaviours in prefabs/scenes.
- **Invalid Unity Events** — checks persistent UnityEvent listeners for missing targets or methods.

Select the built-in asset checks in the window. They share one asset traversal; each still has its own checkbox. You can add additional asset checks by implementing `IAssetValidator` in an Editor assembly; the factory discovers them by reflection. Asset checks receive one object at a time, so they do not need to search the AssetDatabase or open scenes themselves.

Field attributes are opt-in and go on serialized fields:

```csharp
using GValidator.NotNulls.Attributes;
using GValidator.Strings.Attributes;
using UnityEngine;

public sealed class CharacterSettings : ScriptableObject
{
    [SerializeField, NotNull] GameObject _prefab;
    [SerializeField, StringNotEmpty] string _displayName;
}
```

## Add custom validation

### Validate a MonoBehaviour or ScriptableObject with `IValidable`

Implement `IValidable` on a runtime `MonoBehaviour` or `ScriptableObject`. The IValidable asset check calls `Validate` and associates messages with that object.

```csharp
using GValidator.Validation.Builder;
using GValidator.Validation.Validables;
using UnityEngine;

public sealed class SpawnPoint : MonoBehaviour, IValidable
{
    [SerializeField] Transform _spawnTransform;

    public void Validate(IValidationBuilder validation)
    {
        if (_spawnTransform == null)
        {
            validation.Error("Spawn Transform is not assigned");
        }
    }
}
```

Use `validation.Error(...)`, `Warning(...)`, and `Info(...)` to report messages. `IValidationBuilder` is in the runtime assembly and is safe to use from runtime code.

### Add a discovered asset validator

For a check that receives each object from the Assets dispatcher, implement `IAssetValidator` in an Editor-only assembly. Implement `CanValidate` to select object types and `ValidateAsync` to check them. Concrete implementations are discovered by reflection and need a parameterless constructor. Annotate them with `[Validator("Name shown in results")]` to choose the displayed name.

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

The dispatcher handles source/scope filtering, object context, progress, and frame slicing. In long checks, call `context.FrameSlicer.TrySlice()` periodically. Implement `IValidator` only for standalone checks that need to manage their own workflow.

### Reuse the ScriptableObject validator base

For a validator dedicated to one ScriptableObject type, inherit from `ScriptableObjectValidator<TScriptableObject>`:

```csharp
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validators.ScriptableObjects;

[Validator("Character Settings")]
public sealed class CharacterSettingsValidator : ScriptableObjectValidator<CharacterSettings>
{
    protected override void Validate(CharacterSettings asset, IValidationBuilder validation)
    {
        if (asset.StartingHealth <= 0)
        {
            validation.Error("Starting Health must be positive");
        }
    }
}
```

The Assets dispatcher passes matching ScriptableObjects to the base, which calls your `Validate` method. Put the concrete validator in an Editor-only assembly.

### Group validators under a parent

If your validator runs a family of child validators, inherit from `ValidatorWithChildren<TChild>` and implement `IValidator.ValidateAsync`. Set `TChild` to the interface or base type of its children (for example, `IAssetValidator`). The factory discovers matching validators by reflection, and the base class stores `Children` and `DisabledChildren`; `EnabledChildren` is derived automatically. Before each run the selection provider updates `DisabledChildren`, so your validator only needs to iterate `EnabledChildren`. `ValidationContext` does not carry selection state. Disabling a parent skips it without changing individual child selections. The built-in Assets validator is an example of this pattern.
