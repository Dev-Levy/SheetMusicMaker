using System.Xml.Linq;

namespace AnalyzerService
{
    public static class MusicAnalyzer
    {
        public static void Analyze(Models.Recording recording, string input)
        {
            XDocument doc = XDocument.Load("template.xml");

            //analysis goes brrrrrrrrrrrrrrrrrrrr.......

            doc.Save(input);
        }
    }
}
