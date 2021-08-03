using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfUploder.Data;
using PdfUploder.Services;
using PdfUploder.Validators;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfUploder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {        
        public IDocumentContext _context { get; }
        public IDocumentFactory _documentFactory { get; }
        public IPdfValidator _validator { get; }

        public DocumentController(IDocumentContext context, IDocumentFactory documentFactory, IPdfValidator validator)
        {          
            _context = context;
            _documentFactory = documentFactory;
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
            
            var content = new MemoryStream(result.File);
            var contentType = "APPLICATION/octet-stream";
            var fileName = "download.pdf";
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await _context.DeleteAsync(id) ? Ok() : NotFound();
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> Reorder()
        {
            return await _context.ChangeSortOrder() ? Ok() : BadRequest();
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
            newDoc.Name = new Guid().ToString();
            await _context.CreateAsync(newDoc);

            return Created(new Uri($"{Request.Path}/{newDoc.Id}", UriKind.Relative), newDoc);
        }
    }
}