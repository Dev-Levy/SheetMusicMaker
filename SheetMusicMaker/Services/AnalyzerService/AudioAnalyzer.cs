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
            Complex[][] fftFrames = ComputeFftOnFrames(frames, frameSize);

            //calculate frequency, RMS, spectral flux
            float[] frequencies = ComputeFundamentalFreqsOnFrames(fftFrames, sampleRate);
            float[] rms = ComputeRmsOnFrames(frames);
            float[] flux = ComputeSpectralFluxOnFrames(fftFrames);

            List<NoteHelper> detectedNotes = DetectNotes(frequencies, rms, flux);

            foreach (NoteHelper note in detectedNotes)
                Console.WriteLine($"{note.Name} - lenght: {note.FramesCount}");

            Note[] notes = CreateNotes(detectedNotes, audioInfo.Bpm, sampleRate, hopSize, divisions);

            return notes;
        }

        #region Helper Methods
        public float[] HannWindow(int size)
        {
            float[] window = new float[size];
            for (int i = 0; i < size; i++)
                window[i] = 0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (size - 1)));
            return window;
        }

        public void ApplyWindow(float[] frame, float[] window)
        {
            for (int i = 0; i < frame.Length; i++)
                frame[i] *= window[i];
        }

        public float[] ReadAudioSamples(string filePath, out int sampleRate, out int channels)
        {
            using var reader = new AudioFileReader(filePath);
            int sampleCount = (int)reader.Length / (reader.WaveFormat.BitsPerSample / 8);
            float[] samples = new float[sampleCount];
            int read = reader.Read(samples, 0, sampleCount);

            sampleRate = reader.WaveFormat.SampleRate;
            channels = reader.WaveFormat.Channels;

            return samples;
        }

        public float[] ConvertToMono(float[] samples, int channels)
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

        public float[] BandPassFilter(float[] samples, int sampleRate, float lowCut = 50f, float highCut = 5000f)
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

        public float[][] FrameSamples(float[] samples, int frameSize, int hopSize)
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

        public float[] ComputeRmsOnFrames(float[][] frames)
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

        public Complex[][] ComputeFftOnFrames(float[][] frames, int frameSize)
        {
            float[] hannWindow = HannWindow(frameSize);

            Complex[][] fftValues = [.. frames.Select(frame =>
            {
                ApplyWindow(frame, hannWindow);
                return FFT(frame);
            })];

            return fftValues;
        }

        public float[] ComputeSpectralFluxOnFrames(Complex[][] fftFrames)
        {
            int fixThreshold = int.Parse(configuration["Analysis:FluxThreshold"] ?? throw new ArgumentException("Flux threshold missing"));

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

            int window = 10;
            float[] enhancedFluxValues = new float[fluxValues.Count];

            for (int i = 0; i < fluxValues.Count; i++)
            {
                int start = Math.Max(0, i - window);
                int end = Math.Min(fluxValues.Count - 1, i + window);

                float sum = fluxValues[start..(end + 1)].Sum();
                int count = end - start + 1;

                float localMean = sum / count;
                enhancedFluxValues[i] = Math.Max(0, fluxValues[i] - localMean);
            }

            float[] filteredFluxValues = new float[fluxValues.Count];
            for (int i = 0; i < enhancedFluxValues.Length; i++)
            {
                filteredFluxValues[i] = enhancedFluxValues[i] < fixThreshold ? 0 : enhancedFluxValues[i];
            }

            return filteredFluxValues;
        }

        public float[] ComputeFundamentalFreqsOnFrames(Complex[][] fftFrames, int sampleRate)
        {
            float[] freqs = [.. fftFrames.Select(frame =>
            {
                float[] magnitudes = [.. frame.Select(complex => (float)complex.Magnitude)];
                return GetFundamentalFrequencyHPS(magnitudes, sampleRate);
            })];

            return freqs;
        }

        public Complex[] FFT(float[] frame)
        {
            Complex[] fft = new Complex[frame.Length];
            for (int i = 0; i < frame.Length; i++)
                fft[i] = new Complex(frame[i], 0);

            Fourier.Forward(fft, FourierOptions.Matlab);
            return fft;
        }

        public float GetFundamentalFrequencyHPS(float[] magnitudes, int sampleRate, int maxHarmonics = 5)
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
            int windowSize = int.Parse(configuration["Analysis:SmoothingWindowSize"] ?? throw new ArgumentException("SmoothingWindowSize missing"));

            for (int i = 0; i < fundamentalFreqs.Length; i++)
                Console.WriteLine($"{fundamentalFreqs[i]:F2} - rms: {rms[i]:F6} - flux: {flux[i]:F2}");

            List<NoteHelper> notes = SetupNotes(fundamentalFreqs, rms);
            notes = SmoothenNoteGlitches(notes, windowSize);

            for (int i = 0; i < fundamentalFreqs.Length; i++)
                Console.WriteLine($"{notes[i].Frequency:F2}({notes[i].Name}) - rms: {rms[i]:F6} - flux: {flux[i]:F2}");

            List<int> pitchChanges = PitchChangePicking(notes);
            List<int> onsets = PeakPicking(flux);

            int tolerance = 3;

            List<NoteHelper> noteEvents = [];
            NoteHelper lastNote = new();

            for (int i = 0; i < notes.Count; i++)
            {
                if (lastNote.Name is null)
                {
                    lastNote = notes[i];
                    continue;
                }

                bool newNoteStarts = false;

                if (pitchChanges.Contains(i))
                {
                    newNoteStarts = true;
                }
                else
                {
                    bool onsetHere = onsets.Contains(i);
                    bool nearPitchChange = pitchChanges.Any(pc => Math.Abs(pc - i) <= tolerance);

                    if (onsetHere && !nearPitchChange)
                        newNoteStarts = true;
                }

                if (!newNoteStarts)
                {
                    lastNote.FramesCount++;
                }
                else
                {
                    noteEvents.Add((lastNote));

                    lastNote = notes[i];
                }
            }

            if (lastNote.Name is not null)
            {
                noteEvents.Add((lastNote));
            }

            return noteEvents;
        }

        public List<NoteHelper> SetupNotes(float[] fundamentalFreqs, float[] rms)
        {
            float rmsThreshold = float.Parse(configuration["Analysis:RmsThreshold"] ?? throw new ArgumentException("Rms threshold missing"), CultureInfo.InvariantCulture);
            List<NoteHelper> notes = [];

            for (int i = 0; i < fundamentalFreqs.Length; i++)
            {
                if (rms[i] > rmsThreshold)
                {
                    notes.Add(new NoteHelper
                    {
                        Frequency = fundamentalFreqs[i],
                        Name = FrequencyToNoteName(fundamentalFreqs[i]),
                        FramesCount = 1
                    });
                }
                else
                {
                    notes.Add(new NoteHelper
                    {
                        Frequency = 0,
                        Name = "Rest",
                        FramesCount = 1
                    });
                }
            }

            return notes;
        }

        public List<NoteHelper> SmoothenNoteGlitches(List<NoteHelper> notes, int windowSize)
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

        private List<int> PitchChangePicking(List<NoteHelper> notes)
        {
            List<int> changes = [];
            NoteHelper lastNote = new();

            for (int i = 1; i < notes.Count; i++)
            {
                var note = notes[i];
                if (note.Name != lastNote.Name)
                {
                    changes.Add(i);
                }
                lastNote = note;
            }
            return changes[1..];
        }

        public List<int> PeakPicking(float[] flux)
        {
            int windowSize = int.Parse(configuration["Analysis:PeakPickingWindowSize"] ?? throw new ArgumentException("PeakPickingWindowSize missing"));
            int fixThreshold = int.Parse(configuration["Analysis:FluxThreshold"] ?? throw new ArgumentException("Flux threshold missing"));
            float thresholdOffset = float.Parse(configuration["Analysis:FluxThresholdOffset"] ?? throw new ArgumentException("Flux threshold offset missing"), CultureInfo.InvariantCulture);
            List<int> peaks = [];

            for (int n = 1; n < flux.Length - 1; n++)
            {
                // Compute adaptive threshold
                int start = Math.Max(0, n - windowSize);
                int end = Math.Min(flux.Length, n + windowSize);
                float localMean = flux.Skip(start).Take(end - start).Average();
                float adaptiveThreshold = localMean * thresholdOffset;

                // Peak condition
                if (flux[n] > flux[n - 1] && flux[n] > flux[n + 1] && flux[n] > adaptiveThreshold && flux[n] > fixThreshold)
                {
                    peaks.Add(n);
                }
            }

            return peaks[1..];
        }

        public string FrequencyToNoteName(float freq)
        {
            int midi = (int)Math.Round(69 + 12 * Math.Log(freq / 440.0, 2));
            string[] noteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

            int noteIndex = midi % 12;
            int octave = (midi / 12) - 1;

            return $"{noteNames[noteIndex]}{octave}";
        }

        public Note[] CreateNotes(List<NoteHelper> detectedNotes, int bpm, int sampleRate, int hopSize, int divisions)
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

                Note created;

                if (note.Name == "Rest")
                {
                    created = new Note()
                    {
                        Pitch = new Pitch("R0"),
                        Duration = duration,
                    };
                }
                else
                {
                    created = new Note()
                    {
                        Pitch = new Pitch(note.Name),
                        Duration = duration
                    };
                }

                notes.Add(created);
            }

            return [.. notes];
        }

        #endregion
    }
}
