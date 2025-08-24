using Models;
using Models.MusicXml;

namespace OutputGeneratorService
{
    public interface IMusicXmlConfigurator
    {
        string CreateXml(MediaFile audioFile, AudioInfo audioInfo, Note[] notes);
    }
}