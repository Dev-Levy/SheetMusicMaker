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
            try
            {
                logic.CreateRecording(recording);
            }
            catch
            {
                return BadRequest("asd");
            }

            return Ok("File upload complete!");
        }

        [HttpGet("{id}")]
        public IActionResult Read(int id)
        {
            try
            {
                Recording recording = logic.ReadRecording(id);
                return Ok(recording);
            }
            catch
            {
                return BadRequest("asd");
            }

        }

        [HttpGet]
        public IActionResult ReadAll(int id)
        {
            try
            {
                var recordings = logic.ReadAllRecording();
                return Ok(recordings);
            }
            catch
            {
                return BadRequest("asd");
            }

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                logic.DeleteRecording(id);
                return Ok("Recording deleted!");
            }
            catch
            {
                return BadRequest("asd");
            }

        }
    }
}
