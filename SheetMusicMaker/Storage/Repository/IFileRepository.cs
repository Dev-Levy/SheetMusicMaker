using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public interface IFileRepository
    {
        Task CreateAudioFile(AudioFile file, Stream data);
        Task CreatePdfFile(PdfFile file, Stream data);
        void DeleteAudioFile(int id);
        void DeletePdfFile(int id);
        IQueryable<AudioFile> ReadAllAudioFile();
        IQueryable<PdfFile> ReadAllPdfs();
        AudioFile ReadAudioFile(int id);
        PdfFile ReadPdfFile(int id);
        XmlFile ReadXmlFile(int createdForId);
        Task StoreAudioFile(AudioFile file);
        Task StorePdfFile(PdfFile file);
        Task StoreXmlFile(XmlFile file);
    }
}