using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IBusinessLogic
    {
        Task<int> AnalyzeAudioFile(AudioInfo audioInfo);
        void DeleteAudioFile(int id);
        IQueryable<AudioFile> ReadAllAudioFiles();
        AudioFile ReadAudioFile(int id);
        PdfFile ReadPdfFile(int id);
        XmlFile ReadXmlFile(int id);
        Task UploadFile(AudioFile file, Stream stream);
    }
}