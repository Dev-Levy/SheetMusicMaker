using Microsoft.Extensions.Configuration;
using Models.MusicXml;
using System.Xml.Linq;

namespace MusicXmlService
{
    public class MusicXmlConfigurator(IConfiguration configuration) : IMusicXmlConfigurator
    {
        private readonly XDocument doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Data\\template.xml"));
        private readonly int divisions = int.Parse(configuration["XmlConstants:Divisions"] ?? throw new ArgumentException("Config is faulty! Divisions not found!"));
        private readonly int key_fifths = int.Parse(configuration["XmlConstants:KeyFifths"] ?? throw new ArgumentException("Config is faulty! KeyFifths not found!"));
        private readonly string key_mode = configuration["XmlConstants:KeyMode"] ?? throw new ArgumentException("Config is faulty! KeyMode not found!");
        private readonly int time_beats = int.Parse(configuration["XmlConstants:Beats"] ?? throw new ArgumentException("Config is faulty! Beats not found!"));
        private readonly int time_beat_type = int.Parse(configuration["XmlConstants:BeatType"] ?? throw new ArgumentException("Config is faulty! BeatType not found!"));

        public void SetTitle(string title)
        {
            XElement? titleElement = doc.Descendants("work-title").FirstOrDefault();
            if (titleElement is not null)
            {
                titleElement.Value = title;
            }
        }

        public void SetComposer(string composer)
        {
            var composerElement = doc.Descendants("creator")
                .FirstOrDefault(e => e.Attribute("type")?.Value == "composer");
            if (composerElement != null)
            {
                composerElement.Value = composer;
            }
        }

        public void AddNotes(Note[] notes)
        {
            foreach (Note note in notes)
            {
                AddNote(note);
            }
        }

        private void AddNote(Note note)
        {
            XElement part = doc.Descendants("part").FirstOrDefault() ?? throw new InvalidOperationException("No <part> element found.");
            bool fitsInLastMeasure = false;
            if (fitsInLastMeasure)
            {

            }
            //if it fits in the last it should be placed there
            //else new measure is needed and it should be placed there
        }

        private void AddMeasure()
        {
            XElement part = doc.Descendants("part").FirstOrDefault() ?? throw new InvalidOperationException("No <part> element found.");

            int measureNumber = GetLastMeasureNumber() + 1;

            var newMeasure = new XElement("measure",
                new XAttribute("number", measureNumber),
                new XElement("attributes",
                    new XElement("divisions", divisions),
                    new XElement("key",
                        new XElement("fifths", key_fifths),
                        new XElement("mode", key_mode)
                    ),
                    new XElement("time",
                        new XElement("beats", time_beats),
                        new XElement("beat-type", time_beat_type)
                    )
                )
            );

            part.Add(newMeasure);
        }

        private int GetLastMeasureNumber()
        {
            int lastMeasure = doc.Descendants("measure")
                        .Select(m => int.TryParse(m.Attribute("number")?.Value, out var num) ? num : 0)
                        .DefaultIfEmpty(0)
                        .Max();

            return lastMeasure;
        }

        private XElement ConvertNoteToXml(Note note)
        {
            int duration = note.Duration switch
            {
                NoteType.Whole => divisions * 32,
                NoteType.Half => divisions * 16,
                NoteType.Quarter => divisions * 8,
                NoteType.Eight => divisions * 4,
                NoteType.Sixteenth => divisions * 2,
                NoteType.Thirtysecond => divisions,
                _ => throw new NotImplementedException()
            };

            XElement xNote = new("note",
                new XElement("pitch",
                    new XElement("step", note.Step),
                    new XElement("octave", note.Octave)
                ),
                new XElement("duration", duration)
            );
            return xNote;
        }
        public void Save(string outputPath)
        {
            doc.Save(outputPath);
        }
    }
}
