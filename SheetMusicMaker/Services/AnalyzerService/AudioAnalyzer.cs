using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using MusicXmlService;

namespace AnalyzerService
{
    public class AudioAnalyzer(IConfiguration configuration) : IAudioAnalyzer
    {
        public string AnalyzeAndCreateXML(MediaFile audioFile)
        {
            IMusicXmlConfigurator xmlConfigutator = new MusicXmlConfigurator(configuration);

            string outputDir = configuration["FileStorage:CreatedDir"] ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");
            Directory.CreateDirectory(outputDir);

            string xmlName = Path.ChangeExtension(audioFile.FileName, ".xml");
            string xmlPath = Path.Combine(outputDir, xmlName);

            //could use aubio
            List<List<double>> frames = NativeAnalyzer.Analyze(audioFile.FilePath, configuration);
            foreach (var frame in frames)
                Console.WriteLine(string.Join(", ", frame));

            //get the peaks
            //choose the lowest -> frequency

            //read BPM

            //convert to note


            //--------------------CPP part----------------                         DONE
            //read samples -> framing -> windowing -> FFT -> convert to notes -> create XML

            Note[] notes = [
                new Note{ Pitch = new Pitch{Step = "B", Octave = 4}, Duration = 12},
                new Note{ Pitch = new Pitch{Step = "A", Octave = 4}, Duration = 2},
                new Note{ Pitch = new Pitch{Step = "G", Octave = 4}, Duration = 16},
                new Note{ Pitch = new Pitch{Step = "E", Octave = 4}, Duration = 8},
            ];

            xmlConfigutator.SetTitle(Path.GetFileNameWithoutExtension(xmlName));
            xmlConfigutator.SetComposer("Oláh Levente");

            xmlConfigutator.AddNotes(notes);

            xmlConfigutator.Save(xmlPath);
            return xmlPath;
        }

        public async Task<string> ConvertXmlToPdfAsync(string xmlPath)
        {
            string musescorePath = configuration["MuseScorePath"] ?? throw new ArgumentException("Config is faulty! MuseScorePath not found!");
            string outputDir = configuration["FileStorage:CreatedDir"] ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");
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
