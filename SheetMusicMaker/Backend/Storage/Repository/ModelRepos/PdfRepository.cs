using Models;
using Repository.Generics;
using System;
using System.Linq;

namespace Repository.ModelRepos
{
    public class PdfRepository(SheetMusicMakerDBContext ctx) : Repository<Pdf>(ctx)
    {
        public override Pdf Read(int id)
        {
            return ctx.SheetsOfMusic.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new NullReferenceException("No pdf found with this Id!");
        }

        public override void Update(Pdf item)
        {
            var old = Read(item.Id);
            old.Name = item.Name;
            old.Content = item.Content;
            old.CreatedAt = item.CreatedAt;
        }
    }
}
