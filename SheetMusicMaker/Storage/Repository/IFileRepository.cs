using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public interface IFileRepository
    {
        Task CreateFile(MediaFile file, Stream data);
        void DeleteAudioFile(int id);
        void DeletePdfFile(int id);
        IQueryable<MediaFile> ReadAllAudioFile();
        IQueryable<MediaFile> ReadAllPdfs();
        MediaFile ReadAudioFile(int id);
        MediaFile ReadPdfFile(int id);
        void UpdateFile(MediaFile item);
    }
}