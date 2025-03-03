using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController(IPdfLogic pdfLogic) : ControllerBase
    {
        [HttpGet("{id}")]
        public Pdf Read(int id)
        {
            return pdfLogic.Read(id);
        }
    }
}
