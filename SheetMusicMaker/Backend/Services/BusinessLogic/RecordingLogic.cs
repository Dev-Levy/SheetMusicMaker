using BusinessLogic.Interfaces;
using Models;
using Repository.Generics;
using System.Linq;

namespace BusinessLogic
{
    public class RecordingLogic(IRepository<Recording> repository) : IRecordingLogic
    {
        #region CRUD
        public void Create(Recording rec)
        {
            //create logic
            repository.Create(rec);
        }

        public Recording Read(int id)
        {
            //read logic
            return repository.Read(id);
        }

        public IQueryable<Recording> RealAll()
        {
            return repository.ReadAll();
        }

        public void Update(Recording rec)
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
