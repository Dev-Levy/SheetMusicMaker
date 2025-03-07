using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingController(ILogic logic) : ControllerBase
    {
        [HttpGet("{id}")]
        public Recording Read(int id)
        {
            return logic.ReadRecording(id);
        }
    }
}
