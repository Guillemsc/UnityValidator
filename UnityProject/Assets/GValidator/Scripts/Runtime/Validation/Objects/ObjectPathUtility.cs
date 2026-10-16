#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validation.Objects
{
    public static class ObjectPathUtility
    {
        public static string GetPath(Object obj)
        {
            GameObject? gameObject = obj switch
            {
                GameObject target => target,
                Component sourceComponent => sourceComponent.gameObject,
                _ => null
            };

            if (gameObject == null)
            {
                return AssetDatabase.GetAssetPath(obj);
            }

            bool isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(gameObject);
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);
            GameObject? prefabRoot = null;
            if (isPrefabInstance)
            {
                prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            }
            else if (isPrefabAsset)
            {
                prefabRoot = gameObject.transform.root.gameObject;
            }

            Object? prefabAsset = null;
            if (prefabRoot != null)
            {
                prefabAsset = isPrefabInstance
                    ? PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot)
                    : prefabRoot;
            }

            string prefabAssetPath = prefabAsset != null
                ? AssetDatabase.GetAssetPath(prefabAsset)
                : string.Empty;
            bool hasPrefabAssetPath = !string.IsNullOrEmpty(prefabAssetPath);

            string basePath;
            Transform hierarchyRoot;
            if (hasPrefabAssetPath)
            {
                basePath = prefabAssetPath;
                hierarchyRoot = prefabRoot!.transform;
            }
            else
            {
                bool sceneIsValid = gameObject.scene.IsValid();
                bool sceneHasPath = !string.IsNullOrEmpty(gameObject.scene.path);
                bool useScenePath = sceneIsValid && sceneHasPath;

                basePath = useScenePath
                    ? gameObject.scene.path
                    : AssetDatabase.GetAssetPath(gameObject);
                hierarchyRoot = gameObject.transform.root;
            }

            List<string> hierarchyParts = new();
            Transform? current = gameObject.transform;
            while (current != null)
            {
                hierarchyParts.Insert(0, current.name);

                bool reachedHierarchyRoot = current == hierarchyRoot;
                if (reachedHierarchyRoot)
                {
                    break;
                }

                current = current.parent;
            }

            string hierarchyPath = string.Join("/", hierarchyParts);
            if (obj is Component component)
            {
                hierarchyPath = $"{hierarchyPath}/{component.GetType().Name}";
            }

            if (string.IsNullOrEmpty(basePath))
            {
                return hierarchyPath;
            }

            return $"{basePath}/{hierarchyPath}";
        }
    }
}
#endif
