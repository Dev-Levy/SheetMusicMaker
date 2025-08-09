using Models;
using System.IO;
using System.Linq;

namespace BusinessLogic
{
    public interface IBusinessLogic
    {
        void DeleteAudioFile(int id);
        IQueryable<MediaFile> ReadAllAudioFiles();
        MediaFile ReadAudioFile(int id);
        void UploadFile(Stream audioStream, string filename);
    }
}