using Models;
using Repository.Generics;
using System.Linq;

namespace BusinessLogic
{
    public class Logic(IRepository<Recording> recRepository, IRepository<Pdf> pdfRepository) : ILogic
    {
        #region CRUD Recording
        public void CreateRecording(Recording rec)
        {
            //create logic
            recRepository.Create(rec);
        }

        public Recording ReadRecording(int id)
        {
            //read logic
            return recRepository.Read(id);
        }

        public IQueryable<Recording> RealAllRecording()
        {
            return recRepository.ReadAll();
        }

        public void UpdateRecording(Recording rec)
        {
            //update logic
            recRepository.Update(rec);
        }

        public void DeleteRecording(int id)
        {
            recRepository.Delete(id);
        }
        #endregion
        #region CRUD PDF
        public void CreatePdf(Pdf rec)
        {
            //create logic
            pdfRepository.Create(rec);
        }

        public Pdf ReadPdf(int id)
        {
            //read logic
            return pdfRepository.Read(id);
        }

        public IQueryable<Pdf> RealAllPdf()
        {
            return pdfRepository.ReadAll();
        }

        public void UpdatePdf(Pdf rec)
        {
            //update logic
            pdfRepository.Update(rec);
        }

        public void DeletePdf(int id)
        {
            pdfRepository.Delete(id);
        }
        #endregion
    }
}
