using Models;
using Repository;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class BusinessLogic(IFileRepository mediaFileRepo) : IBusinessLogic
    {
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

    }

    public static class AnalyzerLib
    {
        [DllImport("AnalyzerService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double AddNumbers(double a, double b);

        [DllImport("AnalyzerService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Hypotenuse(double a, double b);
    }
}
