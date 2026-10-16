using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.Shaders
{
    [Validator("Material Shader")]
    public sealed class MaterialShaderValidator : IAssetValidator
    {
        public bool CanValidate(Object asset)
        {
            return asset is Material;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            Material material = (Material)asset;
            Shader shader = material.shader;
            if (shader == null)
            {
                validation.Error("Material does not have a shader assigned");
                return Task.CompletedTask;
            }

            ShaderMessage[] messages = ShaderUtil.GetShaderMessages(shader);
            foreach (ShaderMessage shaderMessage in messages)
            {
                bool isError = shaderMessage.severity == ShaderCompilerMessageSeverity.Error;
                if (!isError) continue;

                bool hasSourceLocation = !string.IsNullOrWhiteSpace(shaderMessage.file) && shaderMessage.line > 0;
                string location = hasSourceLocation
                    ? $" ({shaderMessage.file}:{shaderMessage.line})"
                    : string.Empty;
                validation.Error($"Material uses a shader with compilation error: {shaderMessage.message}{location}");
            }

            return Task.CompletedTask;
        }
    }
}
