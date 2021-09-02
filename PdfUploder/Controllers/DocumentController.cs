using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfUploder.Data;
using PdfUploder.Services;
using PdfUploder.Validators;
using System;
using System.Threading.Tasks;

namespace PdfUploder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentContext _context;
        private readonly IDocumentFactory _documentFactory;
        private readonly ICustomFileFactory _fileFactory;
        private readonly IPdfValidator _validator;      

        public DocumentController(IDocumentContext context,
            IDocumentFactory documentFactory,
            ICustomFileFactory content,
            IPdfValidator validator)
        {
            _context = context;
            _documentFactory = documentFactory;
            _fileFactory = content;
            _validator = validator;
        }

        [HttpGet("{order}")]
        public async Task<IDocument[]> Get(bool order)
        {
            return await _context.GetAllAsync(order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _context.GetAsync(id);

            if (result.File == null)
                return NotFound();

            var file = _fileFactory.Create(result);

            return File(file.Stream, file.Type, file.FileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (await _context.DeleteAsync(id))
                return Ok();

            return NotFound();
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> Reorder()
        {
            if (await _context.ChangeSortOrder())
                return Ok();

            return BadRequest("request is incorrect");
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile formFile)
        {
            var results = _validator.CustomValidate(formFile);

            if (!results.IsValid)
            {
                var errors = results.Errors;

                string errorMessages = errors[0].ErrorMessage;

                return BadRequest(errorMessages);
            }

            var newDoc = _documentFactory.Create(formFile.OpenReadStream());

            await _context.CreateAsync(newDoc);

            return Created(new Uri($"{Request.Path}/{newDoc.Id}", UriKind.Relative), newDoc);
        }
    }
}