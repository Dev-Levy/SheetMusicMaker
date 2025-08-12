namespace MusicXmlConfiguringService
{
    public interface IMusicXmlConfigurator
    {
        void Save(string outputPath);
        void SetComposer(string composer);
        void SetTitle(string title);
    }
}