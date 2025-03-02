using System.Linq;

namespace Repository.Generics
{
    public abstract class Repository<T>(SheetMusicMakerDBContext ctx) : IRepository<T> where T : class
    {
        protected SheetMusicMakerDBContext ctx = ctx;

        public void Create(T item)
        {
            ctx.Set<T>().Add(item);
            ctx.SaveChanges();
        }

        public IQueryable<T> ReadAll()
        {
            return ctx.Set<T>();
        }

        public void Delete(int id)
        {
            ctx.Set<T>().Remove(Read(id));
            ctx.SaveChanges();
        }

        public abstract T Read(int id);
        public abstract void Update(T item);
    }
}
