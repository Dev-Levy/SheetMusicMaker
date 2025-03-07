using BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.IO;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingController(ILogic logic) : ControllerBase
    {
        [HttpPost]
        public IActionResult Create([FromForm] IFormFile file)
        {
            try
            {
                Stream stream = file.OpenReadStream();
                byte[] content = new byte[file.Length];

                stream.Read(content, 0, (int)file.Length);
                logic.CreateRecording(new Recording()
                {
                    FileName = file.FileName,
                    SampleRate = 44100,
                    Samples = content
                });
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
        public IActionResult ReadAll()
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
