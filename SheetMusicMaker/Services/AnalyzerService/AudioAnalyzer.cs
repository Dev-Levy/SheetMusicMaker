using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using MusicXmlService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AnalyzerService
{
    public class AudioAnalyzer(IConfiguration configuration) : IAudioAnalyzer
    {
        public string AnalyzeAndCreateXML(MediaFile audioFile)
        {
            int frameSize = int.Parse(configuration["FFT:FrameSize"] ?? throw new ArgumentException("FrameSize missing"));
            int hopSize = int.Parse(configuration["FFT:HopSize"] ?? throw new ArgumentException("HopSize missing"));

            //read samples
            float[] samples = AudioFunctions.ReadAudioSamples(audioFile.FilePath, out int sampleRate, out int channels);

            //convert to mono
            samples = AudioFunctions.ConvertToMono(samples, channels);

            //filter samples
            samples = AudioFunctions.BandPassFilter(samples, sampleRate);

            //framing
            float[][] frames = AudioFunctions.FrameSamples(samples, frameSize, hopSize);

            float[] fundamentalFreqs = AudioFunctions.RetrieveFundamentalFreqs(frames, frameSize, sampleRate);

            List<NoteHelper> smoothedNotes = AudioFunctions.ConvertToNotes(fundamentalFreqs);

            foreach (NoteHelper note in smoothedNotes)
                Console.WriteLine($"{note.Frequency} - ({note.Name})");

            List<NoteHelper> noteEvents = AudioFunctions.AggregateNotes(smoothedNotes);

            foreach (NoteHelper noteEvent in noteEvents)
                Console.WriteLine($"{noteEvent.Name} - lenght: {noteEvent.FramesCount}");

            int bpm = 120; //TODO

            Note[] notes = AudioFunctions.CreateNotes(noteEvents, bpm, sampleRate, frameSize);

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
