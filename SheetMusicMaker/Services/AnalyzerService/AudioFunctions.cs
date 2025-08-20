using MathNet.Numerics.IntegralTransforms;
using Models.MusicXml;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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

    public class AudioFunctions
    {
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

        public static float[] RetrieveFundamentalFreqs(float[][] frames, int frameSize, int sampleRate)
        {
            List<float> freqs = [];

            float[] hannWindow = HannWindow(frameSize);

            foreach (float[] frame in frames)
            {
                //windowing
                AudioFunctions.ApplyWindow(frame, hannWindow);

                //FFT
                Complex[] fft = FFT(frame);

                //get magnitudes
                float[] magnitudes = [.. fft.Select(c => (float)c.Magnitude)];

                //get frequency
                freqs.Add(GetFundamentalFrequencyHPS(magnitudes, sampleRate));
            }

            return [.. freqs];
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

        internal static List<NoteHelper> ConvertToNotes(float[] fundamentalFreqs, int windowSize = 5)
        {
            List<NoteHelper> notes = [.. fundamentalFreqs.Select(freq => new NoteHelper
                                            {
                                                Frequency = freq,
                                                Name = FrequencyToNoteName(freq),
                                                FramesCount = 1
                                            })];


            for (int i = 0; i < notes.Count; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(notes.Count - 1, i + windowSize / 2);
                var window = notes.GetRange(start, end - start + 1);
                NoteHelper mostCommonNote = window.GroupBy(n => n.Name)
                                           .OrderByDescending(g => g.Count())
                                           .First() //group
                                           .First();//element

                notes[i] = mostCommonNote;
            }
            return notes;
        }

        public static string FrequencyToNoteName(float freq)
        {
            int midi = (int)Math.Round(69 + 12 * Math.Log(freq / 440.0, 2));
            string[] noteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

            int noteIndex = midi % 12;
            int octave = (midi / 12) - 1;

            return $"{noteNames[noteIndex]}{octave}";
        }

        public static List<NoteHelper> AggregateNotes(List<NoteHelper> notes)
        {
            const string UNSET = "unset";
            List<NoteHelper> noteEvents = [];

            NoteHelper lastNote = new() { Name = UNSET };

            foreach (NoteHelper note in notes)
            {
                if (note.Name == lastNote.Name)
                {
                    lastNote.FramesCount++;
                }
                else
                {
                    if (lastNote.Name != UNSET)
                    {
                        noteEvents.Add((lastNote));
                    }

                    lastNote = note;
                }
            }

            if (lastNote.Name != UNSET)
            {
                noteEvents.Add((lastNote));
            }

            return noteEvents;
        }

        internal static Note[] CreateNotes(List<NoteHelper> noteEvents, int bpm, int sampleRate, int frameSize)
        {
            double Tframe = frameSize / sampleRate;
            double tempo = 60 / bpm;




            Note[] notes = [
                new Note{ Pitch = new Pitch{Step = "B", Octave = 4}, Duration = 12},
                new Note{ Pitch = new Pitch{Step = "A", Octave = 4}, Duration = 2},
                new Note{ Pitch = new Pitch{Step = "G", Octave = 4}, Duration = 16},
                new Note{ Pitch = new Pitch{Step = "E", Octave = 4}, Duration = 8},
            ];

            return notes;
        }
    }
}
