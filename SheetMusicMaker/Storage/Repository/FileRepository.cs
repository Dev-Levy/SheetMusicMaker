using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class FileRepository(SheetMusicMakerDBContext ctx, IConfiguration configuration) : IFileRepository
    {

        public async Task CreateAudioFile(AudioFile file, Stream data)
        {
            file.FilePath = await CreateFile(file, data);
            await StoreAudioFile(file);
        }

        public async Task CreatePdfFile(PdfFile file, Stream data)
        {
            file.FilePath = await CreateFile(file, data);
            await StorePdfFile(file);
        }

        public async Task<string> CreateFile(MediaFile file, Stream data)
        {
            string uploadDir = configuration["FileStorage:UploadDir"] ?? throw new ArgumentException("Config is faulty! UploadDir not found!");
            string filePath = Path.Combine(uploadDir, file.FileName);

            if (data.CanSeek)
                data.Position = 0;
            await using FileStream fileStream = File.Create(filePath);
            await data.CopyToAsync(fileStream);

            return filePath;
        }

        public async Task StoreAudioFile(AudioFile file)
        {
            await ctx.AudioFiles.AddAsync(file);
            await ctx.SaveChangesAsync();
        }

        public async Task StoreXmlFile(XmlFile file)
        {
            await ctx.XmlFiles.AddAsync(file);
            await ctx.SaveChangesAsync();
        }

        public async Task StorePdfFile(PdfFile file)
        {
            await ctx.PdfFiles.AddAsync(file);
            await ctx.SaveChangesAsync();
        }

        public AudioFile ReadAudioFile(int id)
        {
            return ctx.AudioFiles.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new ArgumentException($"No audio file found with this Id! ({id})");
        }

        public XmlFile ReadXmlFile(int createdForId)
        {
            return ctx.XmlFiles.FirstOrDefault(r => r.CreatedForId.Equals(createdForId)) ?? throw new ArgumentException($"No xml file found created for this Id! ({createdForId})");
        }

        public PdfFile ReadPdfFile(int id)
        {
            return ctx.PdfFiles.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new ArgumentException($"No pdf file found with this Id! ({id})");
        }

        public IQueryable<AudioFile> ReadAllAudioFile()
        {
            return ctx.AudioFiles;
        }

        public IQueryable<PdfFile> ReadAllPdfs()
        {
            return ctx.PdfFiles;
        }

        public void DeleteAudioFile(int id)
        {
            AudioFile file = ReadAudioFile(id);
            DeleteFromDbAndFileSys(file);
        }

        public void DeletePdfFile(int id)
        {
            PdfFile file = ReadPdfFile(id);
            DeleteFromDbAndFileSys(file);
        }

        private void DeleteFromDbAndFileSys(MediaFile file)
        {
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }

            if (file is AudioFile)
                ctx.AudioFiles.Remove(file as AudioFile);
            else if (file is PdfFile)
                ctx.PdfFiles.Remove(file as PdfFile);

            ctx.SaveChanges();
        }
    }
}
