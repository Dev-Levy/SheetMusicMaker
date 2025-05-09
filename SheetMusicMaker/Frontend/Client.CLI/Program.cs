using BusinessLogic;
using Repository;
using Repository.ModelRepos;
using System;

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

            Console.WriteLine("Starting process! (Main)");
            logic.Analyze(id: 3);
        }
    }
}
