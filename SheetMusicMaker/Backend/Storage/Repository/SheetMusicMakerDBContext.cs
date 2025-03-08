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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pdf>().HasData(
                new Pdf()
                {
                    Id = 1,
                    Name = "BENIKE.pdf",
                    Url = "C:\\Users\\horga\\Documents\\Clean.Code.A.Handbook.of.Agile.Software.Craftsmanship.pdf",
                    CreatedAt = new System.DateTime(2003, 10, 12)
                },
                new Pdf()
                {
                    Id = 2,
                    Name = "LEVIKE.pdf",
                    Url = "C:\\Users\\horga\\Documents\\jogviszony.pdf",
                    CreatedAt = new System.DateTime(2003, 10, 16)
                });
        }
    }
}
