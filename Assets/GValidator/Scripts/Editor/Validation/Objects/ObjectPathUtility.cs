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
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject == null)
            {
                return AssetDatabase.GetAssetPath(obj);
            }

            string basePath = gameObject.scene.IsValid() && !string.IsNullOrEmpty(gameObject.scene.path)
                ? gameObject.scene.path
                : AssetDatabase.GetAssetPath(gameObject);

            List<string> hierarchyParts = new();
            Transform? current = gameObject.transform;
            while (current != null)
            {
                hierarchyParts.Insert(0, current.name);
                current = current.parent;
            }

            string hierarchyPath = string.Join("/", hierarchyParts);
            if (string.IsNullOrEmpty(basePath))
            {
                return hierarchyPath;
            }

            return $"{basePath}/{hierarchyPath}";
        }
    }
}
