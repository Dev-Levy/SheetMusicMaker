namespace Models.Music
{
    public class MeasureAttributes
    {
        public int Divisions { get; set; }
        public Key Key { get; set; }
        public Time Time { get; set; }
        public int Staves { get; set; }

        /// <summary>
        /// Creates deafult measure. 4/4 time, C major, single staff.
        /// </summary>
        public MeasureAttributes()
        {
            Divisions = 4;
            Key = new Key(); // Defaults to C major
            Time = new Time(); // Defaults to 4/4
            Staves = 1; // Single staff by default
        }
    }

    public class Key
    {
        public int Fifths { get; set; }
        public string Mode { get; set; } // Optional: Can be "major" or "minor"
        public Key()
        {
            Fifths = 0;
            Mode = "major";
        }
    }

    public class Time
    {
        public int Beats { get; set; }
        public int BeatType { get; set; }

        public Time()
        {
            Beats = 4;
            BeatType = 4;
        }
    }
}
