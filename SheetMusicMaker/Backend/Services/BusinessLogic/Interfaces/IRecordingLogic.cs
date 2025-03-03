using Models;
using System.Linq;

namespace BusinessLogic.Interfaces
{
    public interface IRecordingLogic
    {
        void Create(Recording rec);
        void Delete(int id);
        Recording Read(int id);
        IQueryable<Recording> RealAll();
        void Update(Recording rec);
    }
}