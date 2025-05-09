using Models;
using Models.Music;

namespace AnalyzerService
{
    public class MusicXmlMaker
    {
        public static void MakeXML(Recording recording, string outputPath)
        {
            int beats = 4;


            List<Note> notes = MusicAnalyzer.MakeNotes(recording.Url);
            Measure[] measures = new Measure[notes.Count / beats];


            XmlExporter exporter = new();




            exporter.SaveXML(outputPath);
        }
    }
}
