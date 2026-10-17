# Contributing to GValidator

Contributions are welcome! Bug reports, fixes, new validation rules, documentation improvements, and examples all help make GValidator better for Unity developers. First-time open-source contributors are welcome too.

## Choose a target version

**Every contribution should target a specific GValidator version through its `release/x.x.x` branch.** Create your working branch from that release branch and select the same release branch as the base of your pull request.

1. Check the repository's [branches](https://github.com/Guillemsc/UnityValidator/branches) and [release notes](https://github.com/Guillemsc/UnityValidator/releases).
2. Choose the release your change is intended for. For a bug fix, confirm that the issue exists on that branch.
3. If you are unsure which version should receive a feature or fix, ask in an issue before starting.

Throughout this guide, `x.x.x` means your chosen version. Replace it with an existing release branch's version; it is not a literal branch name. Use the target branch's source code, README, and Unity version when developing your contribution.

## 1. Discuss the change

Search the [issues](https://github.com/Guillemsc/UnityValidator/issues) and [pull requests](https://github.com/Guillemsc/UnityValidator/pulls) for related work.

- **Bug reports:** describe the expected and actual behavior, reproduction steps, Unity version, and GValidator version or Git revision. Include relevant Console messages or a small reproduction project when useful.
- **Features or larger changes:** open an issue describing the problem, proposed approach, and target version so the scope can be discussed.
- **Small fixes or documentation improvements:** feel free to open a pull request directly and explain the change there.

You can contribute by reporting an issue even if you do not plan to implement a fix.

## 2. Fork and clone the repository

A fork is your own copy of the repository on GitHub. Click **Fork** on the [GValidator repository](https://github.com/Guillemsc/UnityValidator), then clone your fork locally.

Replace `YOUR_USERNAME` with your GitHub username:

```sh
git clone https://github.com/YOUR_USERNAME/UnityValidator.git
cd UnityValidator
git remote add upstream https://github.com/Guillemsc/UnityValidator.git
git fetch upstream
```

`origin` points to your fork. `upstream` points to the original GValidator repository.

## 3. Create a branch for your change

Create a descriptive working branch from your chosen release branch:

```sh
git switch -c fix/describe-the-fix upstream/release/x.x.x
```

Use a name such as `fix/missing-reference-report`, `feature/new-validator`, or `docs/setup-guide`. Keep each branch and pull request focused on one change.

## 4. Set up the Unity project

1. Check `UnityProject/ProjectSettings/ProjectVersion.txt` on your target branch for the Unity Editor version to use.
2. In Unity Hub, add the repository's `UnityProject` folder as a project and open it with that Editor version.
3. Let Unity finish importing packages and compiling scripts.
4. Open **Window → GValidator → Validation Window** to try the tool.

The package is already included in this project. The main locations are:

| Location | Purpose |
| --- | --- |
| `UnityProject/Assets/GValidator/Scripts/Runtime` | Runtime-safe attributes, interfaces, settings, and result types |
| `UnityProject/Assets/GValidator/Scripts/Editor` | Editor window, validators, asset discovery, and CI integration |
| `UnityProject/Assets/GValidator/Scripts/Tests/Editor` | EditMode tests |
| `UnityProject/Assets/GValidator/Examples` | Example assets and scripts |
| `README.md` | End-user setup and usage guide |
| `CHANGELOG.md` | User-facing changes and upgrade notes |

## 5. Make and verify your changes

- Follow the surrounding code's naming, formatting, and organization.
- Keep Unity Editor APIs in Editor-only code and runtime code free of Editor dependencies.
- Preserve existing Unity `.meta` files and include the corresponding `.meta` files for new assets, scripts, and folders.
- Keep generated files such as `Library`, `Temp`, and local IDE files out of your commits.
- Update the README or examples when setup, behavior, or public APIs change. Add a concise entry under **Unreleased** in the changelog for user-facing changes.
- Leave package version changes to the release process; the target release is identified by your pull request's base branch.

For code changes, reproduce the issue or exercise the new behavior in the Editor. Add or update meaningful EditMode tests for behavior changes, especially regression tests for bug fixes. Run the relevant tests from **Window → General → Test Runner → EditMode** (the menu location may vary by Unity version).

For documentation changes, check the instructions, links, and code examples. Before committing, review the diff:

```sh
git diff
git diff --check
```

## 6. Commit and push to your fork

Stage the files that belong to your change, then commit with a short, descriptive message:

```sh
git add path/to/changed-file
git commit -m "Fix missing reference reporting"
git push -u origin fix/describe-the-fix
```

Replace the file path and branch name with your own. Include associated `.meta` files when applicable.

## 7. Open a pull request

On GitHub, open a pull request from your fork to `Guillemsc/UnityValidator`. Check the branch selectors carefully:

| Selector | Value |
| --- | --- |
| Base repository | `Guillemsc/UnityValidator` |
| Base branch | The `release/x.x.x` branch you started from |
| Head repository | Your fork |
| Compare branch | Your working branch |

In the pull request description, include:

- **Target version:** the release branch receiving the change.
- **Problem and solution:** what changed and why, with a link to the issue if there is one.
- **Verification:** the Unity version used, tests run, and manual checks performed. Mention anything you could not verify.
- **User impact:** screenshots for UI changes and upgrade instructions for API or configuration changes.

Use a **draft pull request** if you want feedback while the work is still in progress.

## 8. Respond to review

Maintainers may ask questions or request adjustments. Push additional commits to the same branch in your fork; the pull request updates automatically.

If the target release branch changes while your pull request is open, bring those changes into your working branch and rerun the relevant checks:

```sh
git fetch upstream
git merge upstream/release/x.x.x
git push
```

Resolve any merge conflicts locally before pushing. Check the pull request's automated results as well. Unity CI requires license secrets, which are not normally available to pull requests from forks; if checks are blocked for that reason, describe your local verification and ask a maintainer how to proceed.

Once the contribution is reviewed and accepted, a maintainer can merge it into the targeted release branch. Thank you for helping improve GValidator!
