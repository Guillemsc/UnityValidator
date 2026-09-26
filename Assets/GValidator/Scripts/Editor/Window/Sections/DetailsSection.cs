using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Messages;
using UnityEditor;
using UnityEngine;

namespace GValidator.Sections
{
    public sealed class DetailsSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedValidationMessageProvider _selectedValidationMessageProvider;

        public DetailsSection(
            GValidatorWindowReferences references, 
            SelectedValidationMessageProvider selectedValidationMessageProvider)
        {
            _references = references;
            _selectedValidationMessageProvider = selectedValidationMessageProvider;

            selectedValidationMessageProvider.OnValidationMessageChanged += OnValidationMessageChanged;
        }

        void OnValidationMessageChanged(ValidationMessage validationMessage)
        {
            _references.DetailMessageLabel.text = validationMessage.Message ?? string.Empty;
            _references.DetailValidatorName.text = validationMessage.ValidatorName;
            _references.DetailObjectPath.text = string.IsNullOrWhiteSpace(validationMessage.ObjectPath)
                ? GetObjectPath(validationMessage.Object)
                : validationMessage.ObjectPath;
        }

        static string GetObjectPath(Object? obj)
        {
            if (obj == null)
            {
                return "—";
            }

            GameObject? gameObject = obj switch
            {
                GameObject target => target,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject != null && gameObject.scene.IsValid() && !string.IsNullOrEmpty(gameObject.scene.path))
            {
                string hierarchyPath = gameObject.name;
                Transform? parent = gameObject.transform.parent;
                while (parent != null)
                {
                    hierarchyPath = $"{parent.name}/{hierarchyPath}";
                    parent = parent.parent;
                }

                return $"{gameObject.scene.path}/{hierarchyPath}";
            }

            string assetPath = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(assetPath) ? "—" : assetPath;
        }
    }
}
