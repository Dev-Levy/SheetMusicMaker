using BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
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
            string mimeType = GetMimeType(file.FileName);

            return File(fileBytes, mimeType);
        }

        [HttpDelete(audio + "/{id}")]
        public IActionResult DeleteAudioFile(int id)
        {
            logic.DeleteAudioFile(id);

            return NoContent();
        }

        [HttpPost(audio)]
        public async Task<IActionResult> UploadAudioAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            MediaFile mediaFile = new()
            {
                FileName = file.FileName,
                UploadDate = DateTime.Now,
                MediaType = MediaType.Audio
            };

            await using Stream stream = file.OpenReadStream();
            await logic.UploadFile(mediaFile, stream);

            return Ok("Audio file uploaded successfully!");
        }

        [HttpGet("analyze/{id}")]
        public async Task<IActionResult> AnalyzeAudioFile(int id)
        {
            int createdPdfId = await logic.AnalyzeAudioFile(id);

            MediaFile pdfFile = logic.ReadPdfFile(createdPdfId);

            byte[] fileBytes = System.IO.File.ReadAllBytes(pdfFile.FilePath);
            string mimeType = GetMimeType(pdfFile.FileName);

            return File(fileBytes, mimeType, pdfFile.FileName);
        }

        private static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".pdf" => "application/pdf",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }
}
