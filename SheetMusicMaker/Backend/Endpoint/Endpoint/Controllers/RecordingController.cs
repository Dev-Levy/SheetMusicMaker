using BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingController(ILogic logic) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            var checkResult = CheckFileValidity(file);
            if (checkResult is not null)
            {
                return checkResult;
            }

            try
            {
                string filePath = await logic.StoreRecording(file.FileName, file.OpenReadStream());

                logic.CreateRecording(new Recording()
                {
                    FileName = file.FileName,
                    Url = filePath
                });

                return Ok(new { Message = "File uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private IActionResult? CheckFileValidity(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (file.ContentType != "audio/wav" && !Path.GetExtension(file.FileName).Equals(".wav", StringComparison.CurrentCultureIgnoreCase))
            {
                return BadRequest("Invalid file format. Only WAV files are supported.");
            }

            return null;
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
