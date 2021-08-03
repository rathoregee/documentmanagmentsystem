using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace PdfUploder.Validators
{
    public interface IPdfValidator
    {
        ValidationResult CustomValidate(IFormFile instance);
    }
}