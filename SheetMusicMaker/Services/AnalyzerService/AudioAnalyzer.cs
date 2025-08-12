using Microsoft.Extensions.Configuration;
using Models;
using System.Diagnostics;

namespace AnalyzerService
{
    public class AudioAnalyzer(IConfiguration configuration) : IAudioAnalyzer
    {
        public string AnalyzeAndCreateXML(MediaFile file)
        {
            //read samples -> framing -> windowing -> FFT -> convert to notes -> create XML
            //this needs cpp calls
            return Path.Combine(AppContext.BaseDirectory, "TestData\\test.xml");
        }

        public async Task<string> ConvertXmlToPdfAsync(string xmlPath)
        {
            string musescorePath = configuration["MuseScorePath"]
                ?? throw new ArgumentException("Config is faulty! MuseScorePath not found!");
            string outputDir = configuration["FileStorage:CreatedDir"]
                ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");
            Directory.CreateDirectory(outputDir);

            string pdfName = Path.ChangeExtension(Path.GetFileName(xmlPath), ".pdf");
            string pdfPath = Path.Combine(outputDir, pdfName);

            ProcessStartInfo startInfo = new()
            {
                FileName = musescorePath,
                ArgumentList = { "-o", pdfPath, xmlPath },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

            List<string> output = [];
            List<string> errors = [];

            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.Add(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) errors.Add(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new OperationCanceledException($"MuseScore failed: {string.Join("\n", errors)}");
            }

            return pdfPath;
        }
    }
}
