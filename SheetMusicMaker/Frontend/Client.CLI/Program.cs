using BusinessLogic;
using Repository;
using Repository.ModelRepos;

namespace Client.CLI
{
    internal class Program
    {
        static void Main()
        {
            SheetMusicMakerDBContext ctx = new();
            RecordingRepository recRepository = new(ctx);
            PdfRepository pdfRepository = new(ctx);
            Logic logic = new(recRepository, pdfRepository);

            logic.Analyze(id: 2);
        }
    }
}
