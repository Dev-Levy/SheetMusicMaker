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
        public string Step { get; set; }

        [XmlElement("octave")]
        public int Octave { get; set; }
        public Pitch() { }
        public Pitch(string name)
        {
            if (name.Length == 2)
            {
                Step = name[..1].ToString();
                Octave = int.Parse(name[1..]);
            }
            else
            {
                Step = name[..2].ToString();
                Octave = int.Parse(name[2..]);
            }
        }
    }

}
