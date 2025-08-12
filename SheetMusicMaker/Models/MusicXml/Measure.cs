using System.Collections.Generic;
using System.Xml.Serialization;

namespace Models.MusicXml
{
    [XmlRoot("measure")]
    public class Measure
    {
        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlElement("attributes")]
        public Attributes Attributes { get; set; }

        [XmlElement("note")]
        public List<Note> Notes { get; set; }
    }

    public class Attributes
    {
        [XmlElement("divisions")]
        public int Divisions { get; set; }

        [XmlElement("time")]
        public TimeSignature Time { get; set; }

        [XmlElement("key")]
        public KeySignature Key { get; set; }
    }

    public class TimeSignature
    {
        [XmlElement("beats")]
        public int Beats { get; set; }

        [XmlElement("beat-type")]
        public int BeatType { get; set; }
    }

    public class KeySignature
    {
        [XmlElement("fifths")]
        public int Fifths { get; set; }

        [XmlElement("mode")]
        public string Mode { get; set; }
    }

    public class Note
    {
        [XmlElement("pitch")]
        public Pitch Pitch { get; set; }

        [XmlElement("duration")]
        public int Duration { get; set; }
    }

    public class Pitch
    {
        [XmlElement("step")]
        public string Step { get; set; }

        [XmlElement("octave")]
        public int Octave { get; set; }
    }

}
