#nullable enable
using GValidator.Validation.AssetsProviders;
using GValidator.Validation.Scope;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GValidator.Tests
{
    public sealed class AssetValidationScopeTests
    {
        const string TestFolder = "Assets/GValidatorScopeTests";

        GameObject? _root;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            AssetDatabase.DeleteAsset(TestFolder);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Assets/")]
        public void FromPath_DefaultPathsSelectAllAssets(string? path)
        {
            AssetValidationScope scope = AssetValidationScope.FromPath(path);

            Assert.That(scope.Kind, Is.EqualTo(AssetValidationScopeKind.AllAssets));
            Assert.That(scope.Path, Is.EqualTo("Assets"));
            Assert.That(scope.DisplayPath, Is.EqualTo("Assets/"));
            Assert.That(scope.SearchInFolders, Is.EqualTo(new[] { "Assets" }));
            Assert.That(scope.TargetAssetPath, Is.Null);
        }

        [Test]
        public void FromPath_NormalizesAssetPathAndUsesContainingFolder()
        {
            AssetValidationScope scope = AssetValidationScope.FromPath(
                @"Assets\Folder\Nested\Asset.asset/");

            Assert.That(scope.Kind, Is.EqualTo(AssetValidationScopeKind.Asset));
            Assert.That(scope.Path, Is.EqualTo("Assets/Folder/Nested/Asset.asset"));
            Assert.That(scope.DisplayPath, Is.EqualTo(scope.Path));
            Assert.That(scope.SearchInFolders, Is.EqualTo(new[] { "Assets/Folder/Nested" }));
            Assert.That(scope.TargetAssetPath, Is.EqualTo(scope.Path));
        }

        [Test]
        public void FromPath_RecognizesExistingFolder()
        {
            AssetDatabase.CreateFolder("Assets", "GValidatorScopeTests");

            AssetValidationScope scope = AssetValidationScope.FromPath(TestFolder);

            Assert.That(scope.Kind, Is.EqualTo(AssetValidationScopeKind.Folder));
            Assert.That(scope.DisplayPath, Is.EqualTo(TestFolder + "/"));
            Assert.That(scope.SearchInFolders, Is.EqualTo(new[] { TestFolder }));
            Assert.That(scope.TargetAssetPath, Is.Null);
        }

        [Test]
        public void FromGameObject_UsesFullHierarchyPath()
        {
            _root = new GameObject("Root");
            GameObject child = new("Child");
            child.transform.SetParent(_root.transform);

            AssetValidationScope scope = AssetValidationScope.FromGameObject(child);

            Assert.That(scope.Kind, Is.EqualTo(AssetValidationScopeKind.GameObject));
            Assert.That(scope.Path, Is.EqualTo("Root/Child"));
            Assert.That(scope.DisplayPath, Does.EndWith("/Root/Child"));
            Assert.That(scope.SearchInFolders, Is.Empty);
            Assert.That(scope.TargetAssetPath, Is.Null);
        }
    }
}
