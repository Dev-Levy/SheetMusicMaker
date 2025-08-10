using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IBusinessLogic
    {
        IQueryable<MediaFile> ReadAllAudioFiles();
        MediaFile ReadAudioFile(int id);
        void DeleteAudioFile(int id);
        Task UploadFile(MediaFile file, Stream stream);
        MediaFile ReadPdfFile(int id);
    }
}