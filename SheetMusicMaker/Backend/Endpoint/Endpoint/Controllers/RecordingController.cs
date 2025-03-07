using BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using NAudio.Wave;

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
                using var stream = file.OpenReadStream();
                using var reader = new WaveFileReader(stream);
                float[] samples = new float[reader.SampleCount];
                for (int i = 0; i < reader.SampleCount; i++)
                {
                    samples[i] = reader.ReadNextSampleFrame()[0];
                }

                logic.CreateRecording(new Recording()
                {
                    FileName = file.FileName,
                    SampleRate = 44100,
                    Samples = samples
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
