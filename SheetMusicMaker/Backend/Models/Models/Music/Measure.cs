using System.Collections.Generic;

namespace Models.Music
{
    public class Measure
    {
        public int Number { get; set; }
        public MeasureAttributes MAttributes { get; set; }
        public List<Note> Notes { get; set; }
        public Measure()
        {
            Number = 0;
            MAttributes = new MeasureAttributes();
            Notes = [];
        }

        public void AddNote(Note note)
        {
            Notes.Add(note);
        }

    }
}
