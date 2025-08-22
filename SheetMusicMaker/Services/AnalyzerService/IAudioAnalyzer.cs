using Models;
using System.Threading.Tasks;

namespace AnalyzerService
{
    public interface IAudioAnalyzer
    {
        string AnalyzeAndCreateXML(MediaFile file, int bpm);
        Task<string> ConvertXmlToPdfAsync(string xmlPath);
    }
}