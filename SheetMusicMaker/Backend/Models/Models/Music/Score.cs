using System.Collections.Generic;

namespace Models.Music
{
    public class Score
    {
        public required ScoreHeader Header { get; set; }
        public required List<Part> Parts { get; set; } = new List<Part>();

        public void AddParts(Part part)
        {
            Parts.Add(part);
        }
    }
}
