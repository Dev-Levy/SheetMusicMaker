using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OutputGeneratorService
{
    public class MusicXmlConfigurator(IConfiguration configuration) : IMusicXmlConfigurator
    {
        private readonly XDocument doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Data\\template.xml"));
        private readonly int divisions = int.Parse(configuration["XmlConstants:Divisions"] ?? throw new ArgumentException("Config is faulty! Divisions not found!"));
        private readonly int key_fifths = int.Parse(configuration["XmlConstants:KeyFifths"] ?? throw new ArgumentException("Config is faulty! KeyFifths not found!"));
        private readonly string key_mode = configuration["XmlConstants:KeyMode"] ?? throw new ArgumentException("Config is faulty! KeyMode not found!");
        private int time_beats = int.Parse(configuration["XmlConstants:Beats"] ?? throw new ArgumentException("Config is faulty! Beats not found!"));
        private int time_beat_type = int.Parse(configuration["XmlConstants:BeatType"] ?? throw new ArgumentException("Config is faulty! BeatType not found!"));
        private readonly string outputDir = configuration["FileStorage:CreatedDir"] ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");

        public string CreateXml(MediaFile audioFile, AudioInfo audioInfo, Note[] notes)
        {
            string xmlName = Path.ChangeExtension(audioFile.FileName, ".musicxml");
            string xmlPath = Path.Combine(outputDir, xmlName);

            SetTitle(audioInfo.Title);
            SetComposer(audioInfo.Composer);
            SetTimeSignature(audioInfo.Beats, audioInfo.BeatType);
            SetTempo(audioInfo.Bpm);

            AddNotes(notes);

            Save(xmlPath);
            return xmlPath;
        }

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

        public void SetTempo(int bpm)
        {
            XElement part = doc.Descendants("part").FirstOrDefault() ?? throw new InvalidOperationException("No <part> element found.");
            AddMeasure(part, bpm);
        }

        public void SetTimeSignature(int beats, int beatType)
        {
            time_beats = beats;
            time_beat_type = beatType;
        }

        public void AddNotes(Note[] notes)
        {
            XElement part = doc.Descendants("part").FirstOrDefault() ?? throw new InvalidOperationException("No <part> element found.");

            foreach (Note note in notes)
            {
                XElement? lastMeasure = part.Elements("measure").LastOrDefault();

                if (lastMeasure is not null && WillNoteFitInLastMeasure(part, note, lastMeasure))
                {
                    lastMeasure.Add(ConvertNoteToXml(note));
                }
                else
                {
                    AddMeasure(part);
                    lastMeasure = part.Elements("measure").LastOrDefault();
                    lastMeasure.Add(ConvertNoteToXml(note));
                }
                //if it fits in the last it should be placed there
                //else new measure is needed and it should be placed there;
            }
        }

        private bool WillNoteFitInLastMeasure(XElement part, Note note, XElement lastMeasure)
        {
            XmlSerializer serializer = new(typeof(Measure));

            using XmlReader reader = lastMeasure.CreateReader();
            Measure measure = (Measure)serializer.Deserialize(reader)!;

            int totalDivisionsPerMeasure = (int)(divisions * measure.Attributes.Time.Beats * (4.0 / measure.Attributes.Time.BeatType));
            int sumDivisionsUsedInLastMeasure = measure.Notes.Sum(note => note.Duration);

            return sumDivisionsUsedInLastMeasure + note.Duration <= totalDivisionsPerMeasure;
        }

        private void AddMeasure(XElement part, int tempo = 0)
        {
            int measureNumber = GetLastMeasureNumber() + 1;
            XElement newMeasure;
            if (tempo != 0)
            {
                newMeasure = new XElement("measure",
                     new XAttribute("number", measureNumber),
                     new XElement("direction",
                         new XAttribute("placement", "above"),
                         new XElement("direction-type",
                             new XElement("words", $"♩ = {tempo}")
                         ),
                         new XElement("sound", new XAttribute("tempo", tempo))
                     ),
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
            }
            else
            {
                newMeasure = new XElement("measure",
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
            }


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

        private static XElement ConvertNoteToXml(Note note)
        {
            XElement xNote;
            if (note.Pitch.Step == "R") //rest
            {
                xNote = new("note",
                    new XElement("rest"),
                    new XElement("duration", note.Duration)
                );
            }
            else
            {
                xNote = new("note",
                    new XElement("pitch",
                        new XElement("step", note.Pitch.Step),
                        new XElement("alter", note.Pitch.Alter),
                        new XElement("octave", note.Pitch.Octave)
                    ),
                    new XElement("duration", note.Duration)
                );
            }

            return xNote;
        }

        public void Save(string outputPath)
        {
            doc.Save(outputPath);
        }


    }
}
