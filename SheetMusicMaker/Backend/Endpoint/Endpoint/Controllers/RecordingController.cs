using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingController(IRecordingLogic recordingLogic) : ControllerBase
    {
        [HttpGet("{id}")]
        public Recording Read(int id)
        {
            return recordingLogic.Read(id);
        }
    }
}
