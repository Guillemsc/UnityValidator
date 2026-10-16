using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public enum AssetValidationScopeKind
    {
        AllAssets,
        Folder,
        Asset,
        GameObject,
    }

    public sealed class AssetValidationScope
    {
        const string AssetsPath = "Assets";

        public string Path { get; }
        public string DisplayPath { get; }
        public string[] SearchInFolders { get; }
        public string? TargetAssetPath { get; }
        public AssetValidationScopeKind Kind { get; }

        public bool IsAllAssets => Kind == AssetValidationScopeKind.AllAssets;

        AssetValidationScope(
            string path,
            string displayPath,
            string[] searchInFolders,
            string? targetAssetPath,
            AssetValidationScopeKind kind)
        {
            Path = path;
            DisplayPath = displayPath;
            SearchInFolders = searchInFolders;
            TargetAssetPath = targetAssetPath;
            Kind = kind;
        }

        public static AssetValidationScope FromPath(string? scopePath = AssetsPath)
        {
            string normalizedPath = string.IsNullOrWhiteSpace(scopePath)
                ? AssetsPath
                : scopePath!.Replace('\\', '/').TrimEnd('/');

            if (normalizedPath == AssetsPath)
            {
                return new AssetValidationScope(
                    AssetsPath,
                    AssetsPath + "/",
                    new[] { AssetsPath },
                    null,
                    AssetValidationScopeKind.AllAssets);
            }

            bool isFolder = AssetDatabase.IsValidFolder(normalizedPath);
            if (isFolder)
            {
                return new AssetValidationScope(
                    normalizedPath,
                    normalizedPath + "/",
                    new[] { normalizedPath },
                    null,
                    AssetValidationScopeKind.Folder);
            }

            string searchFolder = System.IO.Path.GetDirectoryName(normalizedPath)?.Replace('\\', '/') ?? AssetsPath;
            return new AssetValidationScope(
                normalizedPath,
                normalizedPath,
                new[] { searchFolder },
                normalizedPath,
                AssetValidationScopeKind.Asset);
        }

        public static AssetValidationScope FromGameObject(GameObject gameObject)
        {
            string hierarchyPath = GetHierarchyPath(gameObject);
            string scenePath = gameObject.scene.path;
            string sceneDisplayName = string.IsNullOrWhiteSpace(scenePath)
                ? gameObject.scene.name
                : scenePath;

            return new AssetValidationScope(
                hierarchyPath,
                sceneDisplayName + "/" + hierarchyPath,
                System.Array.Empty<string>(),
                null,
                AssetValidationScopeKind.GameObject);
        }

        static string GetHierarchyPath(GameObject gameObject)
        {
            List<string> names = new();
            Transform? current = gameObject.transform;

            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}
