using System.Collections.Generic;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Result;
using GValidator.Validators;

namespace GValidator.Validation.Runner
{
    public interface IValidationRunner
    {
        IValidationResult Run(IValidationContext validationContext);
    }
}