using System;
using System.Diagnostics;

namespace PdfGenerationService
{
    public static class PdfGenerator
    {
        public static bool Generate(string musicXmlPath, string outputPdfPath)
        {
            try
            {
                string musescoreExePath = @"C:\Program Files\MuseScore 4\bin\MuseScore4.exe";

                ProcessStartInfo startInfo = new()
                {
                    FileName = musescoreExePath,
                    Arguments = $"\"{musicXmlPath}\" -o \"{outputPdfPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Console.WriteLine("PDF creation! (MuseScore process call)");
                Process.Start(startInfo);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
