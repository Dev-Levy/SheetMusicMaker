using Models;
using NAudio.Wave;

namespace AnalyzerService
{
    public class MusicAnalyzer
    {
        static readonly string RESULT_FOLDER_PATH = "C:\\Users\\horga\\Documents\\1_PROJEKTMUNKA\\RESULT_FOLDER_SMM";

        static readonly int FRAME_SIZE = 1024;
        static readonly int OVERLAP = 512;
        public static void Analyze(Recording recording, string outputPath)
        {
            float[] samples = GetSamples(recording);
            float[][] frames = GetFrames(samples, FRAME_SIZE, OVERLAP);

            //foreach (var frame in frames)
            //{
            float[] frame = frames[13];
            double[] doubleFrame = Array.ConvertAll(frame, x => (double)x);

            File.WriteAllText(Path.Combine(RESULT_FOLDER_PATH, "frame.txt"),
                            string.Join('\n', doubleFrame));

            var window = new FftSharp.Windows.Hamming();
            window.ApplyInPlace(doubleFrame);

            File.WriteAllText(Path.Combine(RESULT_FOLDER_PATH, "frameWindowed.txt"),
                            string.Join('\n', doubleFrame));

            var spectrum = FftSharp.FFT.Forward(doubleFrame);
            double[] psd = FftSharp.FFT.Power(spectrum);
            double[] freq = FftSharp.FFT.FrequencyScale(psd.Length, 44100);

            File.WriteAllText(Path.Combine(RESULT_FOLDER_PATH, "freq.txt"),
                            string.Join('\n', freq));
            //}

            //building xml
            XmlExporter exporter = new();

            exporter.SaveXML(outputPath);
        }

        private static float[] GetSamples(Recording recording)
        {
            float[] samples;
            using (WaveFileReader reader = new(recording.Url))
            {

                int bytesPerSample = reader.WaveFormat.BitsPerSample / 8;
                int sampleCount = (int)(reader.Length / bytesPerSample / reader.WaveFormat.Channels);

                samples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[i] = reader.ReadNextSampleFrame()[0];
                }
            }
            return samples;
        }

        private static float[][] GetFrames(float[] samples, int frameSize, int overlap)
        {
            int stepSize = frameSize - overlap;
            int numOfFrames = (samples.Length - frameSize) / stepSize + 1;

            float[][] frames = new float[numOfFrames][];

            for (int i = 0; i < numOfFrames; i++)
            {
                int startIndex = i * stepSize;
                frames[i] = new float[frameSize];
                Array.Copy(samples, startIndex, frames[i], 0, frameSize);
            }

            return frames;
        }
    }
}
