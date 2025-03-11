using System.Collections.Generic;

namespace Models.Music
{
    public class Part
    {
        public required string Id { get; set; }
        public required List<Measure> Measures { get; set; } = new List<Measure>();

        public void AddMeasure(Measure measure)
        {
            Measures.Add(measure);
        }
    }
}
