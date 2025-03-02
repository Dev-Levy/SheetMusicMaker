using System.Diagnostics;

namespace PdfGenerationService
{
    public static class PdfGenerator
    {
        public static void Generate(string musicXmlPath, string outputPdfPath)
        {
            string musescoreExePath = @"C:\Program Files\MuseScore 4\bin\MuseScore4.exe";

            ProcessStartInfo startInfo = new()
            {
                FileName = musescoreExePath,
                Arguments = $"\"{musicXmlPath}\" -o \"{outputPdfPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
    }
}
