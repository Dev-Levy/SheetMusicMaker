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
        public async Task CreateFile(MediaFile file, Stream data)
        {
            string uploadDir = configuration["FileStorage:Path"] ?? throw new ArgumentException("Config is faulty!");

            string filePath = Path.Combine(uploadDir, file.FileName);

            Directory.CreateDirectory(uploadDir);
            if (data.CanSeek)
                data.Position = 0;
            await using FileStream fileStream = File.Create(filePath);
            await data.CopyToAsync(fileStream);

            file.FilePath = filePath;

            ctx.Set<MediaFile>().Add(file);
            ctx.SaveChanges();
        }

        public MediaFile ReadAudioFile(int id)
        {
            return ctx.AudioFiles.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new ArgumentException($"No audio file found with this Id! ({id})");
        }

        public MediaFile ReadPdfFile(int id)
        {
            return ctx.PdfFiles.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new ArgumentException($"No pdf file found with this Id! ({id})");
        }

        public IQueryable<MediaFile> ReadAllAudioFile()
        {
            return ctx.AudioFiles;
        }

        public IQueryable<MediaFile> ReadAllPdfs()
        {
            return ctx.PdfFiles;
        }

        public void DeleteAudioFile(int id)
        {
            MediaFile file = ReadAudioFile(id);
            DeleteFromDbAndFileSys(file);
        }

        public void DeletePdfFile(int id)
        {
            MediaFile file = ReadPdfFile(id);
            DeleteFromDbAndFileSys(file);
        }

        private void DeleteFromDbAndFileSys(MediaFile file)
        {
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }
            ctx.Set<MediaFile>().Remove(file);
            ctx.SaveChanges();
        }

        public void UpdateFile(MediaFile item)
        {
            MediaFile old = item.MediaType == MediaType.Pdf ? ReadPdfFile(item.Id) : ReadAudioFile(item.Id);
            old.FileName = item.FileName;
            old.FilePath = item.FilePath;
            old.UploadDate = item.UploadDate;
        }

    }
}
