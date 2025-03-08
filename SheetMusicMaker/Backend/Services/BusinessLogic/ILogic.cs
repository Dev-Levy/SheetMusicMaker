using Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface ILogic
    {
        void CreatePdf(Pdf rec);
        void CreateRecording(Recording rec);
        void DeletePdf(int id);
        void DeleteRecording(int id);
        Pdf ReadPdf(int id);
        Recording ReadRecording(int id);
        IQueryable<Pdf> RealAllPdf();
        IQueryable<Recording> ReadAllRecording();
        void UpdatePdf(Pdf rec);
        void UpdateRecording(Recording rec);
        Task<string> StoreRecording(string filename, Stream fileBytes);
    }
}