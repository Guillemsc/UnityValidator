# Changelog

## [Unreleased]

- Improved setup, custom-validator, CI, and API documentation; added a contributing guide.
- Fixed child-validator defaults when saving and restoring local overrides.

### Upgrade notes

- Replace static `ValidationDefinitionBuilder.Build(...)` calls with the instance builder's `With...(...)` methods and `Build()`.
- Update Editor imports: asset providers moved to `GValidator.Validation.AssetsProviders`, scopes to `GValidator.Validation.Scope`, and window types to `GValidator.ValidatorWindow` subnamespaces.

## [1.1.0-preview.1] - 2026-10-17

- Added shared validator defaults and persistent per-developer overrides.
- Added finding suppressions with reasons, shared across the Editor, XML exports, and CI.
- Added Hierarchy **Validate** for one GameObject and its components.
- Added detection of materials using Unity's internal error shader.
- Fixed premature batch-mode shutdown and CI exception handling.

### Upgrade notes

- Use `GValidator.Attributes` for `[NotNull]` and `[StringNotEmpty]`.
- `GValidatorConfiguration` is now `GValidatorProjectSettings`; commit the settings asset and its `.meta` file to share defaults and suppressions.
- The Git package path is now `/UnityProject/Assets/GValidator`.

## [1.0.0] - 2026-09-27

First stable release.

- Editor validation for projects, folders, assets, prefabs, and scenes, with severity filters and ignored folders.
- Built-in checks for scripts, layers, shaders, UnityEvents, and required serialized fields.
- Custom rules through `IValidable`, `IAssetValidator`, `IValidator`, and validator base classes.
- Cancellable validation, JUnit XML exports, and batch-mode CI integration.

[Unreleased]: https://github.com/Guillemsc/UnityValidator/compare/v1.1.0-preview.1...release/1.1.0
[1.1.0-preview.1]: https://github.com/Guillemsc/UnityValidator/compare/v1.0.0...v1.1.0-preview.1
[1.0.0]: https://github.com/Guillemsc/UnityValidator/releases/tag/v1.0.0
