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
        public Pdf Read(int id)
        {
            return logic.ReadPdf(id);
        }
    }
}
