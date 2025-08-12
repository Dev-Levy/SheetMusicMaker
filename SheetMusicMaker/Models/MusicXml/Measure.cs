using System.Collections.Generic;
using System.Xml.Serialization;

namespace Models.MusicXml
{
    [XmlRoot("measure")]
    public class Measure
    {
        [XmlAttribute("number")]
        public required int Number { get; set; }

        [XmlElement("attributes")]
        public required Attributes Attributes { get; set; }

        [XmlElement("note")]
        public required List<Note> Notes { get; set; }
    }
}
