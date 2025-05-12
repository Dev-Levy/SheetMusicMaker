using AnalyzerService;
using Models;
using PdfGenerationService;
using Repository.Generics;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class Logic(IRepository<Recording> recRepository, IRepository<Pdf> pdfRepository) : ILogic
    {
        static readonly string UPLOAD_FOLDER_PATH = "C:\\Users\\horga\\Documents\\2_PROJEKTMUNKA\\UPLOAD_FOLDER_SMM";
        static readonly string RESULT_FOLDER_PATH = "C:\\Users\\horga\\Documents\\2_PROJEKTMUNKA\\RESULT_FOLDER_SMM";

        #region CRUD Recording
        public void CreateRecording(Recording rec)
        {
            //create logic
            recRepository.Create(rec);
        }

        public Recording ReadRecording(int id)
        {
            //read logic
            return recRepository.Read(id);
        }

        public IQueryable<Recording> ReadAllRecording()
        {
            return recRepository.ReadAll();
        }

        public void UpdateRecording(Recording rec)
        {
            //update logic
            recRepository.Update(rec);
        }

        public void DeleteRecording(int id)
        {
            recRepository.Delete(id);
        }
        #endregion
        #region CRUD PDF
        public void CreatePdf(Pdf rec)
        {
            //create logic
            pdfRepository.Create(rec);
        }

        public Pdf ReadPdf(int id)
        {
            //read logic
            return pdfRepository.Read(id);
        }

        public IQueryable<Pdf> RealAllPdf()
        {
            return pdfRepository.ReadAll();
        }

        public void UpdatePdf(Pdf rec)
        {
            //update logic
            pdfRepository.Update(rec);
        }

        public void DeletePdf(int id)
        {
            pdfRepository.Delete(id);
        }
        #endregion

        public async Task<string> StoreRecording(string filename, Stream fileStream)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException("No file content provided.");
            }

            string filePath = Path.Combine(UPLOAD_FOLDER_PATH, filename);
            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamWriter);
            }

            return filePath;
        }

        public void Analyze(int id)
        {
            //Arrange
            Recording recording = recRepository.Read(id);
            string xmlOutputPath = Path.Combine(RESULT_FOLDER_PATH, recording.FileName + ".xml");
            string pdf = recording.FileName + ".pdf";
            string pdfOutputPath = Path.Combine(RESULT_FOLDER_PATH, pdf);

            //Act
            Console.WriteLine("Calling analyzer! (Logic)");
            MusicXmlMaker.MakeXML(recording, xmlOutputPath);

            Console.WriteLine();
            Console.WriteLine("Generating PDF! (Logic)");
            bool created = PdfGenerator.Generate(xmlOutputPath, pdfOutputPath);

            if (created)
                Console.WriteLine($"Sheet music created at: {pdfOutputPath}");
            else
                Console.WriteLine("Pdf creation failed!");

            //Persist
            pdfRepository.Create(new() { Name = pdf, Url = pdfOutputPath });
        }
    }
}
