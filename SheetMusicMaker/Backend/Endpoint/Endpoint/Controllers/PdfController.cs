using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController(ILogic logic) : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult Read(int id)
        {
            Pdf pdf = logic.ReadPdf(id);

            string name = pdf.Name;
            byte[] bytes = System.IO.File.ReadAllBytes(pdf.Url);

            return File(bytes, "application/pdf", name);
        }
    }
}
