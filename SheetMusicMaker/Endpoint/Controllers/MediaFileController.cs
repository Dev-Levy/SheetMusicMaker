using BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Endpoint.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MediaFileController(IBusinessLogic logic) : ControllerBase
    {
        const string audio = "audio";

        [HttpGet(audio)]
        public IActionResult GetAllAudioFiles()
        {
            IQueryable<MediaFile> files = logic.ReadAllAudioFiles();

            return Ok(files);
        }

        [HttpGet(audio + "/{id}")]
        public IActionResult GetAudioFile(int id)
        {
            MediaFile file = logic.ReadAudioFile(id);

            byte[] fileBytes = System.IO.File.ReadAllBytes(file.FilePath);
            string mimeType = GetMimeType(file.Name);

            return File(fileBytes, mimeType);
        }

        [HttpDelete(audio + "/{id}")]
        public IActionResult DeleteAudioFile(int id)
        {
            logic.DeleteAudioFile(id);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> UploadAudioAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using Stream stream = file.OpenReadStream();
            logic.UploadFile(stream, file.FileName);

            //store file
            //return name, path, id

            return Ok();
        }

        private static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                _ => "application/octet-stream"
            };
        }
    }
}
