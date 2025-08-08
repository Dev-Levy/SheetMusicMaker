using Models;
using System.Linq;

namespace Repository
{
    public interface IFileRepository
    {
        void CreateFile(MediaFile item);
        void DeleteAudioFile(int id);
        void DeletePdfFile(int id);
        IQueryable<MediaFile> ReadAllAudioFile();
        IQueryable<MediaFile> ReadAllPdfs();
        MediaFile ReadAudioFile(int id);
        MediaFile ReadPdfFile(int id);
        void UpdateFile(MediaFile item);
    }
}