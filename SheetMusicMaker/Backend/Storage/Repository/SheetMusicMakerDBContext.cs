using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository
{
    public class SheetMusicMakerDBContext : DbContext
    {
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<Pdf> SheetsOfMusic { get; set; }

        public SheetMusicMakerDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string conn = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MusicAppDB.mdf;Integrated Security=True;MultipleActiveResultSets=true";

                optionsBuilder.UseSqlServer(conn);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    }
}
