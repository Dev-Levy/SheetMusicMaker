using AnalyzerService;
using Microsoft.Extensions.Configuration;
using Models;
using Repository;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class BusinessLogic(IFileRepository mediaFileRepo, IConfiguration configuration) : IBusinessLogic
    {
        public async Task<int> AnalyzeAudioFile(int id)
        {
            IAudioAnalyzer analyzer = new AudioAnalyzer(configuration);

            //read audio
            MediaFile file = ReadAudioFile(id);

            //read samples -> framing -> windowing -> FFT -> convert to notes -> create XML
            string xmlPath = analyzer.AnalyzeAndCreateXML(file);

            //generate PDF
            string pdfPath = await analyzer.ConvertXmlToPdfAsync(xmlPath);

            //store PDF
            MediaFile pdfFile = new()
            {
                FileName = Path.GetFileName(pdfPath),
                UploadDate = DateTime.Now,
                FilePath = pdfPath,
                MediaType = MediaType.Pdf
            };
            await mediaFileRepo.StoreFile(pdfFile);

            //return ID
            return pdfFile.Id;
        }
        #region CRUD
        public IQueryable<MediaFile> ReadAllAudioFiles()
        {
            return mediaFileRepo.ReadAllAudioFile();
        }

        public MediaFile ReadAudioFile(int id)
        {
            return mediaFileRepo.ReadAudioFile(id);
        }

        public MediaFile ReadPdfFile(int id)
        {
            return mediaFileRepo.ReadPdfFile(id);
        }

        public void DeleteAudioFile(int id)
        {
            mediaFileRepo.DeleteAudioFile(id);
        }

        public async Task UploadFile(MediaFile file, Stream stream)
        {
            await mediaFileRepo.CreateFile(file, stream);
        }
        #endregion
    }
}
