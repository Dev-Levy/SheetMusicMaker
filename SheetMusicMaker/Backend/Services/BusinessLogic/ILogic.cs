using Models;
using System.Linq;

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
        IQueryable<Recording> RealAllRecording();
        void UpdatePdf(Pdf rec);
        void UpdateRecording(Recording rec);
    }
}