using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public enum MediaType { Pdf, Audio }
    public class MediaFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string FileName { get; set; }
        public string FilePath { get; set; }
        public required DateTime UploadDate { get; set; }
        public required MediaType MediaType { get; set; }
    }
}
