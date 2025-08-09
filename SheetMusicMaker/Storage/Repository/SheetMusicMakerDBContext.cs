using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;

namespace Repository
{
    public class SheetMusicMakerDBContext : DbContext
    {
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
                    Name = "BENIKE.pdf",
                    FilePath = "C:\\Users\\horga\\Documents\\Clean.Code.A.Handbook.of.Agile.Software.Craftsmanship.pdf",
                    UploadDate = new DateTime(2003, 10, 12),
                    MediaType = MediaType.Pdf
                },
                new MediaFile()
                {
                    Id = 2,
                    Name = "LEVIKE.pdf",
                    FilePath = "C:\\Users\\horga\\Documents\\jogviszony.pdf",
                    UploadDate = new DateTime(2003, 10, 16),
                    MediaType = MediaType.Pdf
                },
                new MediaFile()
                {
                    Id = 3,
                    Name = "piano.wav",
                    FilePath = "C:\\Users\\horga\\Documents\\2_PROJEKTMUNKA\\UPLOAD_FOLDER_SMM\\piano.wav",
                    UploadDate = new DateTime(2003, 10, 12),
                    MediaType = MediaType.Audio
                });
        }
    }
}
