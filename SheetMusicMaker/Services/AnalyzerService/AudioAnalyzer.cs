using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using MusicXmlService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace AnalyzerService
{
    public class AudioAnalyzer(IConfiguration configuration) : IAudioAnalyzer
    {
        public string AnalyzeAndCreateXML(MediaFile audioFile)
        {
            int frameSize = int.Parse(configuration["FFT:FrameSize"] ?? throw new ArgumentException("FrameSize missing"));
            int hopSize = int.Parse(configuration["FFT:HopSize"] ?? throw new ArgumentException("HopSize missing"));

            float[] window = AudioFunctions.HannWindow(frameSize);

            //read samples
            float[] samples = AudioFunctions.ReadAudioSamples(audioFile.FilePath, out int sampleRate, out int channels);

            //convert to mono
            samples = AudioFunctions.ConvertToMono(samples, channels);

            //filter samples
            samples = AudioFunctions.BandPassFilter(samples, sampleRate);

            //framing
            float[][] frames = AudioFunctions.FrameSignal(samples, frameSize, hopSize);

            List<string> noteNames = [];

            foreach (float[] frame in frames)
            {
                //windowing
                AudioFunctions.ApplyWindow(frame, window);

                //FFT
                Complex[] fft = AudioFunctions.FFT(frame);

                float[] magnitudes = [.. fft.Select(c => (float)c.Magnitude)];

                float freq = AudioFunctions.GetFundamentalFrequencyHPS(magnitudes, sampleRate);
                string noteName = AudioFunctions.FrequencyToNoteName(freq);
                Console.WriteLine($"{freq:F4} - ({noteName})");
                noteNames.Add(noteName);
            }

            foreach (var noteEvent in AudioFunctions.AggregateNotes(noteNames))
            {
                Console.WriteLine($"{noteEvent.Note} - lenght: {noteEvent.Length}");
            }
            //készítünk notes[]

            Note[] notes = [
                new Note{ Pitch = new Pitch{Step = "B", Octave = 4}, Duration = 12},
                new Note{ Pitch = new Pitch{Step = "A", Octave = 4}, Duration = 2},
                new Note{ Pitch = new Pitch{Step = "G", Octave = 4}, Duration = 16},
                new Note{ Pitch = new Pitch{Step = "E", Octave = 4}, Duration = 8},
            ];

            IMusicXmlConfigurator xmlConfigutator = new MusicXmlConfigurator(configuration);

            string outputDir = configuration["FileStorage:CreatedDir"] ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");
            Directory.CreateDirectory(outputDir);

            string xmlName = Path.ChangeExtension(audioFile.FileName, ".xml");
            string xmlPath = Path.Combine(outputDir, xmlName);


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
