namespace Models.Music
{
    public class Note
    {
        public required Pitch Pitch { get; set; }
        public required int Duration { get; set; }
        public required string Type { get; set; }
    }

    public class Pitch
    {
        public string Step { get; set; }
        public int Octave { get; set; }

        public Pitch(string noteName)
        {
            if (noteName.Length == 2) //C4
            {
                Step = noteName[..1];
                Octave = int.Parse(noteName[1..]);
            }
            else //C#4
            {
                Step = noteName[..2];
                Octave = int.Parse(noteName[2..]);
            }
        }
    }
}
