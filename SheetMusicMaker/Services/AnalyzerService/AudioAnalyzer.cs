using MathNet.Numerics.IntegralTransforms;
using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using MusicXmlService;
using NAudio.Wave;
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
            string framesDir = configuration["FileStorage:FramesDir"] ?? throw new ArgumentException("FramesDir missing");
            string framesPath = Path.Combine(framesDir, DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".csv");

            float[] window = HannWindow(frameSize);

            float[] samples = ReadAudioSamples(audioFile.FilePath, out int sampleRate);
            float[][] frames = FrameSignal(samples, frameSize, hopSize);

            using (StreamWriter sw = new(framesPath))
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    ApplyWindow(frames[i], window);
                    Complex[] fft = FFT(frames[i]);

                    double[] magnitudes = [.. fft.Select(c => c.Magnitude)];

                    int maxIndex = Array.IndexOf(magnitudes, magnitudes.Max());
                    float fundamentalFreq = maxIndex * sampleRate / (float)frameSize;

                    Console.WriteLine();

                    sw.WriteLine(string.Join(';', magnitudes));
                    Console.WriteLine($"Freq. for {i}.frame: {fundamentalFreq}");
                }
            }

            //read samples -> framing -> windowing -> FFT -> convert to notes -> create XML

            //get the peaks
            //choose the lowest -> frequency
            //read BPM
            //convert to note


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
        static float[] HannWindow(int size)
        {
            float[] window = new float[size];
            for (int i = 0; i < size; i++)
                window[i] = 0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (size - 1)));
            return window;
        }

        static void ApplyWindow(float[] frame, float[] window)
        {
            for (int i = 0; i < frame.Length; i++)
                frame[i] *= window[i];
        }

        static float[] ReadAudioSamples(string filePath, out int sampleRate)
        {
            using var reader = new AudioFileReader(filePath);
            int sampleCount = (int)reader.Length / (reader.WaveFormat.BitsPerSample / 8);
            float[] samples = new float[sampleCount];
            int read = reader.Read(samples, 0, sampleCount);

            sampleRate = reader.WaveFormat.SampleRate;

            return samples;
        }

        static float[] ConvertToMono(float[] samples, int channels)
        {
            if (channels == 1) return samples;

            int monoLength = samples.Length / channels;
            float[] mono = new float[monoLength];

            for (int i = 0; i < monoLength; i++)
            {
                float sum = 0f;
                for (int ch = 0; ch < channels; ch++)
                {
                    sum += samples[i * channels + ch];
                }
                mono[i] = sum / channels;
            }

            return mono;
        }

        static float[][] FrameSignal(float[] samples, int frameSize, int hopSize)
        {
            int frameCount = (samples.Length - frameSize) / hopSize + 1;
            float[][] frames = new float[frameCount][];

            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = new float[frameSize];
                Array.Copy(samples, i * hopSize, frames[i], 0, frameSize);
            }

            return frames;
        }

        static Complex[] FFT(float[] frame)
        {
            Complex[] fft = new Complex[frame.Length];
            for (int i = 0; i < frame.Length; i++)
                fft[i] = new Complex(frame[i], 0);

            Fourier.Forward(fft, FourierOptions.Matlab);
            return fft;
        }

        public float GetFundamentalFrequency(float[] frame, int sampleRate, float minFreq = 50f, float maxFreq = 5000f)
        {
            int N = frame.Length;

            float[] window = HannWindow(N);
            ApplyWindow(frame, window);

            Complex[] fft = FFT(frame);

            // Compute magnitudes
            float[] magnitudes = [.. fft.Select(c => (float)c.Magnitude)];

            // Convert min/max freq to bins
            int minBin = (int)(minFreq * N / sampleRate);
            int maxBin = Math.Min((int)(maxFreq * N / sampleRate), N / 2 - 1);

            // Find peak
            int peakIndex = minBin;
            float peakMag = 0;
            float peakBin;
            for (int i = minBin; i <= maxBin; i++)
            {
                if (magnitudes[i] > peakMag)
                {
                    peakMag = magnitudes[i];
                    peakIndex = i;
                }
            }

            // Parabolic interpolation
            if (peakIndex > 0 && peakIndex < magnitudes.Length - 1)
            {
                float alpha = magnitudes[peakIndex - 1];
                float beta = magnitudes[peakIndex];
                float gamma = magnitudes[peakIndex + 1];

                float p = 0.5f * (alpha - gamma) / (alpha - 2 * beta + gamma);
                peakBin = peakIndex + p;
            }

            float freq = peakBin * sampleRate / N;

            return freq;
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
