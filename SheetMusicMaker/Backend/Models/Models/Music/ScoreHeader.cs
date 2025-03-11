using System.Collections.Generic;

namespace Models.Music
{
    public class ScoreHeader
    {
        public required string Version { get; set; }
        public required Work Work { get; set; }
        public required Identification Identification { get; set; }
        public required PartList PartList { get; set; }
    }

    public class Work
    {
        public required string WorkTitle { get; set; }
    }

    public class Identification
    {
        public required List<Creator> Creators { get; set; } = new List<Creator>();
    }

    public class Creator
    {
        public required string Type { get; set; }
        public required string Name { get; set; }
    }

    public class PartList
    {
        public required List<ScorePart> ScoreParts { get; set; } = new List<ScorePart>();
    }

    public class ScorePart
    {
        public required string Id { get; set; }
        public required string PartName { get; set; }
    }
}
