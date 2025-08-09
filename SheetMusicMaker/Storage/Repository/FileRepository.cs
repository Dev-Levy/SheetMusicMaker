using Models;
using System;
using System.Linq;

namespace Repository
{
    public class FileRepository(SheetMusicMakerDBContext ctx) : IFileRepository
    {
        public void CreateFile(MediaFile item)
        {
            ctx.Set<MediaFile>().Add(item);
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
            var old = item.MediaType == "Pdf" ? ReadPdfFile(item.Id) : ReadAudioFile(item.Id);
            old.Name = item.Name;
            old.FilePath = item.FilePath;
            old.UploadDate = item.UploadDate;
        }
    }
}
