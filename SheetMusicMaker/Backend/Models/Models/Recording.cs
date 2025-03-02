using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Models
{
    public class Recording
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required int SampleRate { get; set; }
        public required string SamplesJson { get; set; }
        public float[] Samples
        {
            get => JsonSerializer.Deserialize<float[]>(SamplesJson) ?? [];
            set => JsonSerializer.Serialize(value);
        }
    }
}
