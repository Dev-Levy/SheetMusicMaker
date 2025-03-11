namespace Models.Music
{
    public class MeasureAttributes
    {
        public required int Divisions { get; set; }
        public required Key Key { get; set; }
        public required Time Time { get; set; }
        public required int Staves { get; set; }
    }

    public class Key
    {
        public required int Fifths { get; set; }
        public required string Mode { get; set; } // Optional: Can be "major" or "minor"
    }

    public class Time
    {
        public required int Beats { get; set; }
        public required int BeatType { get; set; }
    }
}
