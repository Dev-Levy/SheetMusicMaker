using Microsoft.Extensions.Configuration;
using Models;
using Models.MusicXml;
using System;
using System.Collections.Generic;

namespace AnalyzerService
{
    public class AudioAnalyzer(IConfiguration configuration)
    {
        public Note[] AnalyzeNotes(MediaFile audioFile, AudioInfo audioInfo)
        {
            int frameSize = int.Parse(configuration["FFT:FrameSize"] ?? throw new ArgumentException("FrameSize missing"));
            int hopSize = int.Parse(configuration["FFT:HopSize"] ?? throw new ArgumentException("HopSize missing"));
            int divisions = int.Parse(configuration["XmlConstants:Divisions"] ?? throw new ArgumentException("Divisions missing"));

            //read samples
            float[] samples = AudioFunctions.ReadAudioSamples(audioFile.FilePath, out int sampleRate, out int channels);

            //convert to mono
            samples = AudioFunctions.ConvertToMono(samples, channels);

            //filter samples
            samples = AudioFunctions.BandPassFilter(samples, sampleRate);

            //framing
            float[][] frames = AudioFunctions.FrameSamples(samples, frameSize, hopSize);

            float[] frequencies = AudioFunctions.RetrieveFundamentalFreqs(frames, frameSize, sampleRate);

            List<NoteHelper> smoothedNotes = AudioFunctions.ConvertToNotes(frequencies);

            List<NoteHelper> noteEvents = AudioFunctions.AggregateNotes(smoothedNotes);

            Console.Clear();
            foreach (NoteHelper noteEvent in noteEvents)
                Console.WriteLine($"{noteEvent.Name} - lenght: {noteEvent.FramesCount}");

            Note[] notes = AudioFunctions.CreateNotes(noteEvents, audioInfo.Bpm, sampleRate, hopSize, divisions);

            return notes;
        }
    }
}
