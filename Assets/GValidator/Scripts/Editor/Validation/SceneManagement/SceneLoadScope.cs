using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GValidator.Validation.SceneManagement
{
    public sealed class SceneLoadScope : IDisposable
    {
        readonly bool _openedByScope;

        public Scene Scene { get; }

        public SceneLoadScope(string scenePath)
        {
            Scene = SceneManager.GetSceneByPath(scenePath);
            _openedByScope = !Scene.IsValid() || !Scene.isLoaded;

            if (_openedByScope)
            {
                Scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
        }

        public void Dispose()
        {
            bool shouldCloseScene = _openedByScope && Scene.IsValid() && Scene.isLoaded;
            if (shouldCloseScene)
            {
                EditorSceneManager.CloseScene(Scene, true);
            }
        }
    }
}
