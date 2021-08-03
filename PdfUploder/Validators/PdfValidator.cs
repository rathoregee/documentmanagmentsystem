using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfUploder.Validators
{
    public class PdfValidator : AbstractValidator<IFormFile>, IPdfValidator
    {
        int maxContentLength = 1024 * 1024 * 5; //5 MB
        public PdfValidator()
        {
            RuleFor(f => f.Length).ExclusiveBetween(0, maxContentLength)
                .WithMessage($"File length should be greater than 0 and less than 5 MB");

            RuleFor(f => f.FileName).Must(IsPdf).WithMessage("please upload a valid pdf");
        }
        //here to validate when the IFormFile is null
        public ValidationResult CustomValidate(IFormFile instance)
        {
            return instance == null
                ? new ValidationResult(new[] { new ValidationFailure("file", "Please Attach your file") })
                : base.Validate(instance);
        }

        bool IsPdf(string path)
        {
            var pdfString = "%PDF-";
            var pdfBytes = Encoding.ASCII.GetBytes(pdfString);
            var len = pdfBytes.Length;
            var buf = new byte[len];
            var remaining = len;
            var pos = 0;
            using (var f = File.OpenRead(path))
            {
                while (remaining > 0)
                {
                    var amtRead = f.Read(buf, pos, remaining);
                    if (amtRead == 0) return false;
                    remaining -= amtRead;
                    pos += amtRead;
                }
            }
            return pdfBytes.SequenceEqual(buf);
        }
    }
}
