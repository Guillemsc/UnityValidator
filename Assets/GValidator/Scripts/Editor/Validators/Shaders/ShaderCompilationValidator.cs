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
    [Validator("Shader Compilation")]
    public sealed class ShaderCompilationValidator : IAssetValidator
    {
        public bool CanValidate(Object asset)
        {
            return asset is Shader;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            Shader shader = (Shader)asset;
            ShaderMessage[] messages = ShaderUtil.GetShaderMessages(shader);

            foreach (ShaderMessage shaderMessage in messages)
            {
                bool isError = shaderMessage.severity == ShaderCompilerMessageSeverity.Error;
                if (!isError) continue;

                bool hasSourceLocation = !string.IsNullOrWhiteSpace(shaderMessage.file) && shaderMessage.line > 0;
                string location = hasSourceLocation
                    ? $" ({shaderMessage.file}:{shaderMessage.line})"
                    : string.Empty;
                validation.Error($"{shaderMessage.message}{location}");
            }

            return Task.CompletedTask;
        }
    }
}
