using Models.MusicXml;

namespace MusicXmlService
{
    public interface IMusicXmlConfigurator
    {
        void AddNotes(Note[] notes);
        void Save(string outputPath);
        void SetComposer(string composer);
        void SetTitle(string title);
    }
}