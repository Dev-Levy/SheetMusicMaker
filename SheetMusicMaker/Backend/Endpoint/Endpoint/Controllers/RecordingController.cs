using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingController(ILogic logic) : ControllerBase
    {
        [HttpPost]
        public IActionResult Create([FromBody] Recording recording)
        {
            logic.CreateRecording(recording);

            return Ok("File upload complete!");
        }

        [HttpGet("{id}")]
        public IActionResult Read(int id)
        {
            Recording recording = logic.ReadRecording(id);

            return Ok(recording);
        }

        [HttpGet]
        public IActionResult ReadAll(int id)
        {
            var recordings = logic.ReadAllRecording();

            return Ok(recordings);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            logic.DeleteRecording(id);

            return Ok();
        }
    }
}
