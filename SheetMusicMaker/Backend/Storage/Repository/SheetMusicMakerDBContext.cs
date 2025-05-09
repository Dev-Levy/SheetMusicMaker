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
            modelBuilder.Entity<Recording>().HasData(
                //new Recording()
                //{
                //    Id = 1,
                //    FileName = "example",
                //    Url = @"C:\Users\OLL4BP\Downloads\Instrument.wav"
                //}
                new Recording()
                {
                    Id = 1,
                    FileName = "example",
                    Url = @"C:\Users\horga\Documents\2_PROJEKTMUNKA\UPLOAD_FOLDER_SMM\example.wav"
                },
                new Recording()
                {
                    Id = 2,
                    FileName = "440",
                    Url = @"C:\Users\horga\Documents\2_PROJEKTMUNKA\UPLOAD_FOLDER_SMM\440.wav"
                },
                new Recording()
                {
                    Id = 3,
                    FileName = "piano",
                    Url = @"C:\Users\horga\Documents\2_PROJEKTMUNKA\UPLOAD_FOLDER_SMM\piano8.wav"
                }
                );

        }
    }
}
