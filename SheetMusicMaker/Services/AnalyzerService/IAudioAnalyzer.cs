using Models;
using Models.MusicXml;

namespace AnalyzerService
{
    public interface IAudioAnalyzer
    {
        Note[] AnalyzeNotes(MediaFile audioFile, AudioInfo audioInfo);
    }
}