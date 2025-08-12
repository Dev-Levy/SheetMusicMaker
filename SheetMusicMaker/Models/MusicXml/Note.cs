namespace Models.MusicXml
{
    public enum NoteType
    {
        Whole,
        Half,
        Quarter,
        Eight,
        Sixteenth,
        Thirtysecond
    }
    public class Note
    {
        public char Step { get; set; }
        public int Octave { get; set; }
        public NoteType Duration { get; set; }

        public Note(string note, NoteType duration)
        {
            //only works for whole steps (e.g A4, C2)
            //(not ok for D#3)
            Step = note[0];
            Octave = int.Parse(note[1].ToString());
            Duration = duration;
        }

        public Note(char step, int octave, NoteType duration)
        {
            Step = step;
            Octave = octave;
            Duration = duration;
        }
    }
}
