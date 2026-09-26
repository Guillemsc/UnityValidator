---
name: C# Coding Guidelines
description: Follow this Unity project's C# style and coding rules when creating or modifying code.
---

# C# Coding Guidelines

- **Use braces for `if` statements**, except when the body consists only of one `return` or `continue` statement. Put that statement on the same line without braces. If the branch contains any additional logic or statements, use braces.

  ```csharp
  if (asset == null) return;

  if (isInvalid)
  {
      LogInvalidState();
  }
  ```

- Apply the same braced-body style to `else`, `for`, `foreach`, `while`, and other control-flow blocks for consistency.
- **Do not call a method or perform a query directly in a `foreach` collection expression.** Store the result first, then iterate over the local variable on a separate line. This makes collection retrieval explicit and easier to inspect/debug.

  ```csharp
  MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
  foreach (MonoBehaviour behaviour in behaviours)
  {
      // Process behaviour.
  }
  ```

- **Do not add `try`/`catch` blocks by default.** Let exceptions propagate unless there is a specific reason to handle them, such as recovering from a known expected failure, adding useful context at an API boundary, or translating an exception into a defined result. Catch only the exception types that can be handled meaningfully; avoid blanket catches used only to suppress errors. Use `finally` when cleanup must happen regardless of success or failure.
- **Extract complex conditions before `if` statements.** When a condition combines multiple checks or is otherwise difficult to scan, calculate it into a clearly named `bool` local, then branch on that local.

  ```csharp
  bool shouldCloseScene = !wasLoaded && scene.IsValid() && scene.isLoaded;
  if (shouldCloseScene)
  {
      EditorSceneManager.CloseScene(scene, true);
  }
  ```

- **Separate distinct logical concerns with blank lines throughout the code.** Apply this in methods and control-flow blocks: visually group related statements and separate phases such as input retrieval, setup, the main operation, result handling, and cleanup/reporting. This applies to all code, not only loops. Do not put a blank line between every statement; use one where the code moves to a different concern. For example:

  ```csharp
  string normalizedPath = scopePath.Replace('\\', '/');
  string[] searchInFolders = { Path.GetDirectoryName(normalizedPath) ?? "Assets" };

  AssetsProvider assetsProvider = new(sources, searchInFolders);
  ValidationContext context = new(validators, assetsProvider, frameSlicer);

  IValidationResult result = await validationRunner.RunAsync(context);

  currentValidation.Set(result);
  ```

- Follow the surrounding code's naming, namespace, formatting, and file-organization conventions.
- Keep changes focused; preserve unrelated user changes and avoid unrelated refactoring.
- For Unity scripts, preserve existing `.meta` files and add Unity metadata for new scripts/folders when the surrounding project uses it.
- Keep Unity Editor APIs in editor-only code and avoid using Unity APIs from background threads.
- After editing, check the diff for whitespace/style issues and build the affected Unity assembly when the project environment allows it.
