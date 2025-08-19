using MathNet.Numerics.IntegralTransforms;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using Complex = System.Numerics.Complex;

namespace AnalyzerService
{
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

        public static float[][] FrameSignal(float[] samples, int frameSize, int hopSize)
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

        public static string FrequencyToNoteName(float freq)
        {
            int midi = (int)Math.Round(69 + 12 * Math.Log(freq / 440.0, 2));
            string[] noteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

            int noteIndex = midi % 12;
            int octave = (midi / 12) - 1;

            return $"{noteNames[noteIndex]}{octave}";
        }

        public static List<(string Note, int Length)> AggregateNotes(List<string> notes)
        {
            var noteEvents = new List<(string Note, int Length)>();

            string lastNote = null;
            int count = 0;

            foreach (var note in notes)
            {
                if (note == lastNote)
                {
                    count++;
                }
                else
                {
                    if (lastNote != null)
                    {
                        noteEvents.Add((lastNote, count));
                    }

                    lastNote = note;
                    count = 1;
                }
            }

            if (lastNote != null)
            {
                noteEvents.Add((lastNote, count));
            }

            return noteEvents;
        }
    }
}
