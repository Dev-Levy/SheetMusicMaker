using Models;
using Repository;
using Repository.ModelRepos;

namespace Client.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var recording1 = new Recording() { SampleRate = 44100, SamplesJson = """[-0.7215260011951632, -0.07588045874281213, 0.21186210277807715, -0.040237382572468006, 0.9971275226378027, 0.49393531705285687, 0.3294170701450472, 0.13894373490541412, 0.6082752365243496, 0.3672959071141233]""" };
            var pdf1 = new Pdf() { Name = "asd.pdf", Content = [], CreatedAt = new System.DateTime(2003, 10, 16) };

            SheetMusicMakerDBContext ctx = new();
            RecordingRepository recRepo = new(ctx);
            PdfRepository pdfRepo = new(ctx);

            recRepo.Create(recording1);
            pdfRepo.Create(pdf1);

            var rec1 = recRepo.Read(id: 1);
            var sheet1 = pdfRepo.Read(id: 1);
        }
    }
}
