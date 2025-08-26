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

            XmlFile xmlFile = new()
            {
                FileName = Path.GetFileName(xmlPath),
                UploadDate = DateTime.Now,
                FilePath = xmlPath,
                CreatedForId = audioFile.Id
            };
            PdfFile pdfFile = new()
            {
                FileName = Path.GetFileName(pdfPath),
                UploadDate = DateTime.Now,
                FilePath = pdfPath,
                CreatedForId = audioFile.Id
            };

            await mediaFileRepo.StoreXmlFile(xmlFile);
            await mediaFileRepo.StorePdfFile(pdfFile);

            return pdfFile.Id;
        }

        #region CRUD
        public IQueryable<AudioFile> ReadAllAudioFiles()
        {
            return mediaFileRepo.ReadAllAudioFile();
        }

        public AudioFile ReadAudioFile(int id)
        {
            return mediaFileRepo.ReadAudioFile(id);
        }

        public XmlFile ReadXmlFile(int id)
        {
            return mediaFileRepo.ReadXmlFile(id);
        }

        public PdfFile ReadPdfFile(int id)
        {
            return mediaFileRepo.ReadPdfFile(id);
        }

        public void DeleteAudioFile(int id)
        {
            mediaFileRepo.DeleteAudioFile(id);
        }

        public async Task UploadFile(AudioFile file, Stream stream)
        {
            IQueryable<MediaFile> files = ReadAllAudioFiles();
            if (files.Any(audioFile => audioFile.FileName == file.FileName))
            {
                throw new InvalidOperationException($"File with name {file.FileName} already uploaded!");
            }
            await mediaFileRepo.CreateAudioFile(file, stream);
        }
        #endregion
    }
}
