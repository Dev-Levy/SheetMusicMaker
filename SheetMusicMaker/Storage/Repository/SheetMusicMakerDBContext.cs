using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.IO;

namespace Repository
{
    public class SheetMusicMakerDBContext : DbContext
    {
        private readonly string BASE_DIR = AppContext.BaseDirectory;
        public DbSet<AudioFile> AudioFiles { get; set; }
        public DbSet<PdfFile> PdfFiles { get; set; }
        public DbSet<XmlFile> XmlFiles { get; set; }

        public SheetMusicMakerDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string conn = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MyDB.mdf;Integrated Security=True;MultipleActiveResultSets=true";

                optionsBuilder.UseSqlServer(conn);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AudioFile>().HasData(
                new AudioFile()
                {
                    Id = 2,
                    FileName = "piano.wav",
                    FilePath = Path.Combine(BASE_DIR, "Data\\piano.wav"),
                    UploadDate = DateTime.MinValue,
                },
                new AudioFile()
                {
                    Id = 3,
                    FileName = "boci.wav",
                    FilePath = Path.Combine(BASE_DIR, "Data\\boci.wav"),
                    UploadDate = DateTime.MinValue,
                });

            modelBuilder.Entity<PdfFile>().HasData(
                 new PdfFile()
                 {
                     Id = 1,
                     FileName = "Test.pdf",
                     FilePath = Path.Combine(BASE_DIR, "Data\\Test.pdf"),
                     UploadDate = DateTime.MinValue,
                     CreatedForId = 2
                 });
        }
    }
}
