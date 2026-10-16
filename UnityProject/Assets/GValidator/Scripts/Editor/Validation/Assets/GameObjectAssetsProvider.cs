using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validation.Assets
{
    public sealed class GameObjectAssetsProvider : IAssetsProvider
    {
        readonly GameObject _gameObject;

        public GameObjectAssetsProvider(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public Task<List<Object>> GetAssetsAsync(string filter)
        {
            List<Object> assets = new();
            if (_gameObject != null)
            {
                assets.Add(_gameObject);

                Component[] components = _gameObject.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;

                    assets.Add(component);
                }
            }

            return Task.FromResult(assets);
        }
    }
}
