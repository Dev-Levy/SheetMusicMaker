using BusinessLogic.Interfaces;
using Models;
using Repository.Generics;
using System.Linq;

namespace BusinessLogic
{
    public class PdfLogic(IRepository<Pdf> repository) : IPdfLogic
    {
        #region CRUD
        public void Create(Pdf rec)
        {
            //create logic
            repository.Create(rec);
        }

        public Pdf Read(int id)
        {
            //read logic
            return repository.Read(id);
        }

        public IQueryable<Pdf> RealAll()
        {
            return repository.ReadAll();
        }

        public void Update(Pdf rec)
        {
            //update logic
            repository.Update(rec);
        }

        public void Delete(int id)
        {
            repository.Delete(id);
        }
        #endregion
    }
}
