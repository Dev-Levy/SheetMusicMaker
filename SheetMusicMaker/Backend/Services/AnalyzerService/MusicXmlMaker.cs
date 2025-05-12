using Models;
using Models.Music;

namespace AnalyzerService
{
    public class MusicXmlMaker
    {
        public static void MakeXML(Recording recording, string outputPath)
        {
            List<Note> notes = MusicAnalyzer.MakeNotes(recording.Url);
            Measure[] measures = new Measure[notes.Count / 4]; //somehow i gitta get this beats

            for (int i = 0; i < measures.Length; i++)
                measures[i] = new Measure();

            int beats = measures[0].MAttributes.Time.Beats;

            for (int i = 0; i < notes.Count; i++)
                measures[i / beats].AddNote(notes[i]);

            XmlExporter exporter = new();

            for (int i = 0; i < measures.Length; i++)
                exporter.AppendMeasure(measures[i]);

            exporter.SaveXML(outputPath);
        }
    }
}
