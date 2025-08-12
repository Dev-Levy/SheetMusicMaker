using System.Xml.Serialization;

namespace Models.MusicXml
{
    public class Attributes
    {
        [XmlElement("divisions")]
        public required int Divisions { get; set; }

        [XmlElement("time")]
        public required TimeSignature Time { get; set; }

        [XmlElement("key")]
        public required KeySignature Key { get; set; }
    }

    public class TimeSignature
    {
        [XmlElement("beats")]
        public required int Beats { get; set; }

        [XmlElement("beat-type")]
        public required int BeatType { get; set; }
    }

    public class KeySignature
    {
        [XmlElement("fifths")]
        public required int Fifths { get; set; }

        [XmlElement("mode")]
        public required string Mode { get; set; }
    }
}
