using Models;
using Repository;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class BusinessLogic(IFileRepository mediaFileRepo) : IBusinessLogic
    {
        public void AnalyzeAudioFile(int id, out int createdPdfId)
        {
            MediaFile file = ReadAudioFile(id);

            createdPdfId = 1;
        }
        #region CRUD
        public IQueryable<MediaFile> ReadAllAudioFiles()
        {
            return mediaFileRepo.ReadAllAudioFile();
        }

        public MediaFile ReadAudioFile(int id)
        {
            return mediaFileRepo.ReadAudioFile(id);
        }

        public MediaFile ReadPdfFile(int id)
        {
            return mediaFileRepo.ReadPdfFile(id);
        }

        public void DeleteAudioFile(int id)
        {
            mediaFileRepo.DeleteAudioFile(id);
        }

        public async Task UploadFile(MediaFile file, Stream stream)
        {
            await mediaFileRepo.CreateFile(file, stream);
        }
        #endregion
    }
}
