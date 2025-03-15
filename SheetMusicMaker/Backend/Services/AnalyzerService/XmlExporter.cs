using Models.Music;
using System.Xml.Linq;

namespace AnalyzerService
{
    internal class XmlExporter
    {
        private readonly XDocument doc;
        public XmlExporter()
        {
            doc = XDocument.Load("template.xml");
        }

        public void AppendMeasure(Measure measure, string partname = "P1")
        {
            var part = doc.Descendants("part").FirstOrDefault(p => p.Attribute("id")?.Value == partname) ?? throw new NullReferenceException("There are no parts yet in this score!");
            //choosing number
            measure.Number = DecideMeasureNumber(part, measure);

            XElement ser_measure = SerializeMeasure(measure);
            part.Add(ser_measure);
        }
        private int DecideMeasureNumber(XElement part, Measure measure)
        {
            if (!part.Elements("measure").Any())
                return 1;
            else
                return int.Parse(part.Elements("measure")?.Last()?.Attribute("number")?.Value) + 1;
        }
        public void AppendNote(Note note, string measureNum = "-1", string partname = "P1")
        {
            var part = doc.Descendants("part").FirstOrDefault(p => p.Attribute("id")?.Value == partname) ?? throw new NullReferenceException("There are no parts yet in this score!");

            XElement? measure;
            if (measureNum == "-1")
                measure = part.Elements("measure").Last();
            else
                measure = part.Elements("measure").FirstOrDefault(m => m.Attribute("number")?.Value == measureNum);

            if (measure == null)
                throw new NullReferenceException("Measure not found!");

            XElement serNote = SerializeNote(note);
            measure.Add(serNote);
        }

        private XElement SerializeMeasure(Measure measure)
        {
            //attributes
            var res = new XElement("measure",
                new XAttribute("number", measure.Number),
                SerializeMAttributes(measure.MAttributes));

            //notes
            foreach (var note in measure.Notes)
                res.Add(SerializeNote(note));

            return res;
        }
        private XElement SerializeMAttributes(MeasureAttributes attributes)
        {
            return new XElement("attributes",
                new XElement("divisions", attributes.Divisions),
                new XElement("key",
                    new XElement("fifths", attributes.Key.Fifths),
                    new XElement("mode", attributes.Key.Mode)),
                new XElement("time",
                    new XElement("beats", attributes.Time.Beats),
                    new XElement("beat-type", attributes.Time.BeatType)),
                new XElement("staves", attributes.Staves));

        }
        private XElement SerializeNote(Note note)
        {
            return new XElement("note",
                new XElement("pitch",
                    new XElement("step", note.Pitch.Step),
                    new XElement("octave", note.Pitch.Octave)),
                new XElement("duration", note.Duration),
                new XElement("type", note.Type));
        }
        public void SaveXML(string path)
        {
            doc.Save(path);
        }
    }
}
