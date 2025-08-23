
namespace OutputGeneratorService
{
    public interface IPdfGenerator
    {
        Task<string> ConvertXmlToPdfAsync(string xmlPath);
    }
}