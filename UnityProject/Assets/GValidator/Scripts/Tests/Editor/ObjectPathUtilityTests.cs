#nullable enable
using GValidator.Validation.Objects;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GValidator.Tests
{
    public sealed class ObjectPathUtilityTests
    {
        const string TestFolder = "Assets/GValidatorObjectPathTests";
        const string PrefabPath = TestFolder + "/Nested.prefab";

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

        [Test]
        public void GetPath_UsesHierarchyAndComponentTypeForSceneObjects()
        {
            _root = new GameObject("Root");
            GameObject child = new("Child");
            child.transform.SetParent(_root.transform);

            string gameObjectPath = ObjectPathUtility.GetPath(child);
            string componentPath = ObjectPathUtility.GetPath(child.transform);

            Assert.That(gameObjectPath, Is.EqualTo("Root/Child"));
            Assert.That(componentPath, Is.EqualTo("Root/Child/Transform"));
        }

        [Test]
        public void GetPath_UsesPrefabAssetPathAndNestedHierarchy()
        {
            AssetDatabase.CreateFolder("Assets", "GValidatorObjectPathTests");
            _root = new GameObject("PrefabRoot");
            GameObject child = new("Child");
            child.transform.SetParent(_root.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(_root, PrefabPath);
            GameObject prefabChild = prefab.transform.Find("Child").gameObject;

            string path = ObjectPathUtility.GetPath(prefabChild.transform);

            string expectedPath = $"{PrefabPath}/{prefab.name}/Child/Transform";
            Assert.That(path, Is.EqualTo(expectedPath));
        }
    }
}
