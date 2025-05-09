using System.Collections.Generic;

namespace Models.Music
{
    public class Measure
    {
        public int Number { get; set; }
        public required MeasureAttributes MAttributes { get; set; }
        public List<Note> Notes { get; set; } = [];
        public void AddNote(Note note)
        {
            Notes.Add(note);
        }
    }
}
