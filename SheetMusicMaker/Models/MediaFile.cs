using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public abstract class MediaFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string FileName { get; set; }
        public string FilePath { get; set; }
        public required DateTime UploadDate { get; set; }
    }
    public sealed class AudioFile : MediaFile
    {
    }

    public sealed class PdfFile : MediaFile
    {
        public int CreatedForId { get; set; }
    }
    public sealed class XmlFile : MediaFile
    {
        public int CreatedForId { get; set; }
    }
}
