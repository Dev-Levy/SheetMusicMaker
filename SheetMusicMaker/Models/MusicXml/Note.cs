using System.Xml.Serialization;

namespace Models.MusicXml
{
    public class Note
    {
        [XmlElement("pitch")]
        public required Pitch Pitch { get; set; }

        [XmlElement("duration")]
        public required int Duration { get; set; }
    }
    public class Pitch
    {
        [XmlElement("step")]
        public required string Step { get; set; }

        [XmlElement("octave")]
        public required int Octave { get; set; }
    }

}
