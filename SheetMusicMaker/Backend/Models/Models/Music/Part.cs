using System.Collections.Generic;

namespace Models.Music
{
    public class Part
    {
        public string Id { get; set; }
        public List<Measure> Measures { get; set; }

        public Part(string Id)
        {
            this.Id = Id;
            Measures = [];
        }

        public void AddMeasure(Measure measure)
        {
            Measures.Add(measure);
        }
    }
}
