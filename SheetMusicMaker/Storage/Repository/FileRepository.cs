using Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class FileRepository(SheetMusicMakerDBContext ctx) : IFileRepository
    {
        public async Task CreateFile(MediaFile file, Stream data)
        {
            string filePath = Path.Combine(Config.UPLOAD_DIR, file.FileName);

            Directory.CreateDirectory(Config.UPLOAD_DIR);
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
            ctx.Set<MediaFile>().Remove(ReadAudioFile(id));
            ctx.SaveChanges();
        }

        public void DeletePdfFile(int id)
        {
            ctx.Set<MediaFile>().Remove(ReadPdfFile(id));
            ctx.SaveChanges();
        }

        public void UpdateFile(MediaFile item)
        {
            var old = item.MediaType == MediaType.Pdf ? ReadPdfFile(item.Id) : ReadAudioFile(item.Id);
            old.FileName = item.FileName;
            old.FilePath = item.FilePath;
            old.UploadDate = item.UploadDate;
        }
    }
}
