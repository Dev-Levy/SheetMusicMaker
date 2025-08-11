using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.IO;
using System.Linq;

namespace Repository
{
    public class SheetMusicMakerDBContext : DbContext
    {
        private readonly string BASE_DIR = AppContext.BaseDirectory;
        public IQueryable<MediaFile> AudioFiles => MediaFiles.Where(f => f.MediaType == MediaType.Audio);
        public IQueryable<MediaFile> PdfFiles => MediaFiles.Where(f => f.MediaType == MediaType.Pdf);

        public DbSet<MediaFile> MediaFiles { get; set; }

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
            modelBuilder.Entity<MediaFile>().HasData(
                new MediaFile()
                {
                    Id = 1,
                    FileName = "Test.pdf",
                    FilePath = Path.Combine(BASE_DIR, "TestData\\Test.pdf"),
                    UploadDate = DateTime.MinValue,
                    MediaType = MediaType.Pdf
                },
                new MediaFile()
                {
                    Id = 2,
                    FileName = "piano.wav",
                    FilePath = Path.Combine(BASE_DIR, "TestData\\piano.wav"),
                    UploadDate = DateTime.MinValue,
                    MediaType = MediaType.Audio
                });
        }
    }
}
