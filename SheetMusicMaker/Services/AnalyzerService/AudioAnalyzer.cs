using MathNet.Numerics.IntegralTransforms;
using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Complex = System.Numerics.Complex;

namespace AnalyzerService
{
    public struct NoteHelper
    {
        public float Frequency { get; set; }
        public string Name { get; set; }
        public int FramesCount { get; set; }
    }

    public class AudioAnalyzer(IConfiguration configuration) : IAudioAnalyzer
    {
        private static readonly int[] ValidDivisions = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128];

        public Note[] AnalyzeNotes(MediaFile audioFile, AudioInfo audioInfo)
        {
            int frameSize = int.Parse(configuration["FFT:FrameSize"] ?? throw new ArgumentException("FrameSize missing"));
            int hopSize = int.Parse(configuration["FFT:HopSize"] ?? throw new ArgumentException("HopSize missing"));
            int divisions = int.Parse(configuration["XmlConstants:Divisions"] ?? throw new ArgumentException("Divisions missing"));

            //read samples
            float[] samples = ReadAudioSamples(audioFile.FilePath, out int sampleRate, out int channels);

            //convert to mono
            samples = ConvertToMono(samples, channels);

            //filter samples
            samples = BandPassFilter(samples, sampleRate);

            //framing
            float[][] frames = FrameSamples(samples, frameSize, hopSize);
            Complex[][] ffFrames = ComputeFftOnFrames(frames, sampleRate);

            //calculate frequency, RMS, spectral flux
            float[] frequencies = ComputeFundamentalFreqsOnFrames(ffFrames, sampleRate);
            float[] rms = ComputeRmsOnFrames(frames);
            float[] flux = ComputeSpectralFluxOnFrames(ffFrames);

            List<NoteHelper> detectedNotes = DetectNotes(frequencies, rms, flux);

            foreach (NoteHelper note in detectedNotes)
                Console.WriteLine($"{note.Name} - lenght: {note.FramesCount}");

            Note[] notes = CreateNotes(detectedNotes, audioInfo.Bpm, sampleRate, hopSize, divisions);

            return notes;
        }

        public static float[] HannWindow(int size)
        {
            float[] window = new float[size];
            for (int i = 0; i < size; i++)
                window[i] = 0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (size - 1)));
            return window;
        }

        public static void ApplyWindow(float[] frame, float[] window)
        {
            for (int i = 0; i < frame.Length; i++)
                frame[i] *= window[i];
        }

        public static float[] ReadAudioSamples(string filePath, out int sampleRate, out int channels)
        {
            using var reader = new AudioFileReader(filePath);
            int sampleCount = (int)reader.Length / (reader.WaveFormat.BitsPerSample / 8);
            float[] samples = new float[sampleCount];
            int read = reader.Read(samples, 0, sampleCount);

            sampleRate = reader.WaveFormat.SampleRate;
            channels = reader.WaveFormat.Channels;

            return samples;
        }

        public static float[] ConvertToMono(float[] samples, int channels)
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

        public static float[] BandPassFilter(float[] samples, int sampleRate, float lowCut = 50f, float highCut = 5000f)
        {
            var filtered = new float[samples.Length];

            var hp = BiQuadFilter.HighPassFilter(sampleRate, lowCut, 1f);
            var lp = BiQuadFilter.LowPassFilter(sampleRate, highCut, 1f);

            for (int i = 0; i < samples.Length; i++)
            {
                float x = hp.Transform(samples[i]);
                filtered[i] = lp.Transform(x);
            }

            return filtered;
        }

        public static float[][] FrameSamples(float[] samples, int frameSize, int hopSize)
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

        public static float[] ComputeRmsOnFrames(float[][] frames)
        {
            float[] rmsValues = [.. frames.Select(frame =>
            {
                //sum all samples energy square
                double sumSq = frame.Sum(sample => sample * sample);

                //sqrt the thing
                return (float)Math.Sqrt(sumSq / frame.Length);
            })];

            return rmsValues;
        }

        public static Complex[][] ComputeFftOnFrames(float[][] frames, int frameSize)
        {
            float[] hannWindow = HannWindow(frameSize);

            Complex[][] fftValues = [.. frames.Select(frame =>
            {
                ApplyWindow(frame, hannWindow);
                return FFT(frame);
            })];

            return fftValues;
        }

        public static float[] ComputeSpectralFluxOnFrames(Complex[][] fftFrames)
        {
            List<float> fluxValues = [0];

            for (int i = 1; i < fftFrames.Length; i++)
            {
                Complex[] prev = fftFrames[i - 1];
                Complex[] curr = fftFrames[i];
                double flux = 0;
                for (int j = 0; j < curr.Length; j++)
                {
                    double diff = curr[j].Magnitude - prev[j].Magnitude;
                    if (diff > 0) flux += diff;  // only count increases
                }
                fluxValues.Add((float)flux);
            }

            return [.. fluxValues];
        }

        public static float[] ComputeFundamentalFreqsOnFrames(Complex[][] fftFrames, int sampleRate)
        {
            float[] freqs = [.. fftFrames.Select(frame =>
            {
                float[] magnitudes = [.. frame.Select(complex => (float)complex.Magnitude)];
                return GetFundamentalFrequencyHPS(magnitudes, sampleRate);
            })];

            return freqs;
        }

        public static Complex[] FFT(float[] frame)
        {
            Complex[] fft = new Complex[frame.Length];
            for (int i = 0; i < frame.Length; i++)
                fft[i] = new Complex(frame[i], 0);

            Fourier.Forward(fft, FourierOptions.Matlab);
            return fft;
        }

        public static float GetFundamentalFrequencyHPS(float[] magnitudes, int sampleRate, int maxHarmonics = 5)
        {
            int N = magnitudes.Length;
            int halfN = N / 2;

            float[] hps = new float[halfN];
            Array.Copy(magnitudes, hps, halfN);

            for (int h = 2; h <= maxHarmonics; h++)
            {
                for (int i = 0; i < halfN / h; i++)
                {
                    hps[i] *= magnitudes[i * h];
                }
            }

            // Ignore DC (bin 0), search only within a musical range
            int minBin = (int)(50f * N / sampleRate);
            int maxBin = (int)(5000f * N / sampleRate);

            float maxVal = float.MinValue;
            int peakIndex = minBin;

            for (int i = minBin; i <= maxBin; i++)
            {
                if (hps[i] > maxVal)
                {
                    maxVal = hps[i];
                    peakIndex = i;
                }
            }

            return (float)peakIndex * sampleRate / N;
        }

        public List<NoteHelper> DetectNotes(float[] fundamentalFreqs, float[] rms, float[] flux)
        {
            float fluxThreshold = float.Parse(configuration["Analysis:FluxThreshold"]
                               ?? throw new ArgumentException("Flux threshold missing"), CultureInfo.InvariantCulture);
            float rmsThreshold = float.Parse(configuration["Analysis:RmsThreshold"]
                               ?? throw new ArgumentException("Rms threshold missing"), CultureInfo.InvariantCulture);
            int windowSize = int.Parse(configuration["Analysis:SmoothingWindowSize"]
                               ?? throw new ArgumentException("SmoothingWindowSize missing"));

            List<NoteHelper> notes = [.. fundamentalFreqs.Select(freq => new NoteHelper { Frequency = freq, Name = FrequencyToNoteName(freq), FramesCount = 1 })];

            Console.Clear();
            foreach (var note in notes)
            {
                Console.WriteLine($"{note.Frequency} - ({note.Name})");
            }

            notes = SmoothenNoteGlitches(notes, windowSize);

            for (int i = 0; i < notes.Count; i++)
            {
                NoteHelper note = notes[i];
                Console.SetCursorPosition(20, i);
                Console.WriteLine($"{note.Frequency} - ({note.Name})");
            }

            List<NoteHelper> noteEvents = [];
            NoteHelper lastNote = new();

            for (int i = 0; i < notes.Count; i++)
            {
                //bool pitchChanged = lastNote.Name is not null && notes[i].Name != lastNote.Name;
                bool onsetDetected = lastNote.Name is not null && ((flux[i] > fluxThreshold) || (rms[i] - rms[i - 1] > rmsThreshold));

                onsetDetected = false;

                if (notes[i].Name == lastNote.Name && !onsetDetected)
                {
                    lastNote.FramesCount++;
                }
                else
                {
                    if (lastNote.Name is not null)
                    {
                        noteEvents.Add((lastNote));
                    }

                    lastNote = notes[i];
                }
            }

            if (lastNote.Name is not null)
            {
                noteEvents.Add((lastNote));
            }

            return noteEvents;
        }

        public static List<NoteHelper> SmoothenNoteGlitches(List<NoteHelper> notes, int windowSize)
        {
            List<NoteHelper> smoothenedNotes = [];

            for (int i = 0; i < notes.Count; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(notes.Count - 1, i + windowSize / 2);
                var window = notes.GetRange(start, end - start + 1);
                NoteHelper mostCommonNote = window.GroupBy(n => n.Name)
                                           .OrderByDescending(g => g.Count())
                                           .First() //group
                                           .First();//element

                smoothenedNotes.Add(mostCommonNote);
            }
            return smoothenedNotes;
        }

        public static string FrequencyToNoteName(float freq)
        {
            int midi = (int)Math.Round(69 + 12 * Math.Log(freq / 440.0, 2));
            string[] noteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

            int noteIndex = midi % 12;
            int octave = (midi / 12) - 1;

            return $"{noteNames[noteIndex]}{octave}";
        }

        public static Note[] CreateNotes(List<NoteHelper> detectedNotes, int bpm, int sampleRate, int hopSize, int divisions)
        {
            double Tframe = (double)hopSize / (double)sampleRate;
            double Tbeat = 60 / (double)bpm;
            double Tdivision = Tbeat / (double)divisions;

            Console.WriteLine($"One frame is {Tframe:F2}s and one beat is {Tbeat:F2}s and one division is {Tdivision:F2}s");
            Console.WriteLine($"One beat is equal to {divisions} division");

            List<Note> notes = [];

            foreach (NoteHelper note in detectedNotes)
            {
                double Tnote = note.FramesCount * Tframe;
                double BeatsPerNote = Tnote / Tbeat;
                double DivisionsPerNote = Tnote / Tdivision;

                Console.WriteLine($"{note.Name} lasted {Tnote:F2}s = {BeatsPerNote:F2} beats = {DivisionsPerNote} divisions");

                int duration = ValidDivisions.MinBy(div => Math.Abs(DivisionsPerNote - (double)div));
                Console.WriteLine($"Duration in divisions: {duration}");
                if (duration == 0)
                    continue;


                notes.Add(new Note()
                {
                    Pitch = new Pitch(note.Name),
                    Duration = duration
                });
            }

            return [.. notes];
        }
    }
}
