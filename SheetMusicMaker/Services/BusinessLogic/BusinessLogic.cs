using AnalyzerService;
using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using OutputGeneratorService;
using Repository;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class BusinessLogic(IFileRepository mediaFileRepo, IConfiguration configuration) : IBusinessLogic
    {
        public async Task<int> AnalyzeAudioFile(AudioInfo audioInfo)
        {
            IAudioAnalyzer analyzer = new AudioAnalyzer(configuration);
            IMusicXmlConfigurator xmlConfigurator = new MusicXmlConfigurator(configuration);
            IPdfGenerator pdfGenerator = new PdfGenerator(configuration);

            MediaFile audioFile = ReadAudioFile(audioInfo.Id);

            Note[] notes = analyzer.AnalyzeNotes(audioFile, audioInfo);
            string xmlPath = xmlConfigurator.CreateXml(audioFile, audioInfo, notes);
            string pdfPath = await pdfGenerator.ConvertXmlToPdfAsync(xmlPath);

            MediaFile pdfFile = new()
            {
                FileName = Path.GetFileName(pdfPath),
                UploadDate = DateTime.Now,
                FilePath = pdfPath,
                MediaType = MediaType.Pdf
            };
            await mediaFileRepo.StoreFile(pdfFile);

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
