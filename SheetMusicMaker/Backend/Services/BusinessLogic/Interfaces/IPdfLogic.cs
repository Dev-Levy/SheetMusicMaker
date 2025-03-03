using Models;
using System.Linq;

namespace BusinessLogic.Interfaces
{
    public interface IPdfLogic
    {
        void Create(Pdf rec);
        void Delete(int id);
        Pdf Read(int id);
        IQueryable<Pdf> RealAll();
        void Update(Pdf rec);
    }
}