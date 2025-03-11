using System.Collections.Generic;

namespace Models.Music
{
    public class Measure
    {
        public required int Number { get; set; }
        public required MeasureAttributes MAttributes { get; set; }
        public required List<Note> Notes { get; set; } = new List<Note>();
        public void AddNote(Note note)
        {
            Notes.Add(note);
        }
    }
}
