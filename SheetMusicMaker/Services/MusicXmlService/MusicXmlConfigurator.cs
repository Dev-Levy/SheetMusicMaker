using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace MusicXmlConfiguringService
{
    public class MusicXmlConfigurator : IMusicXmlConfigurator
    {
        private readonly XDocument doc;
        private readonly int defaultDivision;
        public MusicXmlConfigurator(IConfiguration configuration)
        {
            doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Data\\template.xml"));
            defaultDivision = int.Parse(configuration["XmlConstants:Divisions"] ?? throw new ArgumentException("Config is faulty! Divisions not found!"));
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

        public void Save(string outputPath)
        {
            doc.Save(outputPath);
        }
    }
}
