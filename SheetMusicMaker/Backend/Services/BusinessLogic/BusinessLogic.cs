using Models;
using Repository;
using System;
using System.IO;
using System.Linq;

namespace BusinessLogic
{
    public class BusinessLogic(IFileRepository mediaFileRepo) : IBusinessLogic
    {
        const string UPLOAD_DIR = "C:\\Users\\horga\\Documents\\2_PROJEKTMUNKA\\UPLOAD_FOLDER_SMM";
        public IQueryable<MediaFile> ReadAllAudioFiles()
        {
            return mediaFileRepo.ReadAllAudioFile();
        }

        public MediaFile ReadAudioFile(int id)
        {
            return mediaFileRepo.ReadAudioFile(id);
        }

        public void DeleteAudioFile(int id)
        {
            mediaFileRepo.DeleteAudioFile(id);
        }

        public async void UploadFile(Stream audioStream, string filename)
        {
            var safeFileName = Path.GetFileName(filename); // avoid path traversal
            var filePath = Path.Combine(UPLOAD_DIR, safeFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await audioStream.CopyToAsync(fileStream);

            MediaFile file = new()
            {
                Name = filename,
                FilePath = filePath,
                UploadDate = DateTime.Now,
                MediaType = MediaType.Audio
            };
            mediaFileRepo.CreateFile(file);
        }
    }
}
