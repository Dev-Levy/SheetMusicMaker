namespace Models.Music
{
    public class Note
    {
        public required Pitch Pitch { get; set; }
        public required int Duration { get; set; }
        public required string Type { get; set; }
    }

    public class Pitch(string step, int octave)
    {
        public required string Step { get; set; } = step;
        public required int Octave { get; set; } = octave;
    }
}
