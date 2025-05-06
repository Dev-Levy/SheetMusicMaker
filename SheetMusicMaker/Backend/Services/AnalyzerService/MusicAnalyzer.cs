using Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AnalyzerService
{
    public record PitchInterval(double PitchHz, double StartTime, double EndTime, double Duration);

    public class MusicAnalyzer
    {
        public static void Analyze(Recording recording, string outputPath)
        {
            Frame[] frames = AnalyzePitch(recording.Url);

            List<(double, double, string)> values = [];
            foreach (var frame in frames)
                values.Add((frame.Time, frame.Pitch, FrequencyToNoteName(frame.Pitch)));

            var groups = values.GroupBy(v => v.Item3);

            Console.ReadLine();
        }

        private static string FrequencyToNoteName(double frequency)
        {
            if (frequency <= 0) return "Silence";

            double midiNote = 69 + 12 * Math.Log2(frequency / 440.0);
            int noteNumber = (int)Math.Round(midiNote);

            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            string noteName = noteNames[noteNumber % 12];

            return $"{noteName}{octave}";
        }

        private static Frame[] AnalyzePitch(string audioPath)
        {
            InstallPackages();

            string pyFile = "pitch_analyzer.py";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string procPyFilePath = Path.Combine(appDirectory, pyFile);

            if (!File.Exists(procPyFilePath))
            {
                throw new FileNotFoundException("Python analyzer process file not found", procPyFilePath);
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "python",  // Or full path like @"C:\Python39\python.exe"
                Arguments = $"{pyFile} \"{audioPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = processInfo };
            process.Start();

            string jsonOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Frame[] frames = JsonConvert.DeserializeObject<Frame[]>(jsonOutput) ?? [];

            return frames;
        }

        private static void InstallPackages()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string batFilePath = Path.Combine(appDirectory, "package_setup.bat");

            if (!File.Exists(batFilePath))
            {
                throw new FileNotFoundException("BAT file not found", batFilePath);
            }

            ProcessStartInfo processInfo = new()
            {
                FileName = batFilePath,
                WorkingDirectory = appDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process proc = new() { StartInfo = processInfo };
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                throw new Exception($"Package installation failed.\nOutput: {output}\nError: {error}");
            }
        }
        public class Frame
        {
            public double Time { get; set; }
            public double Pitch { get; set; }
            public bool IsVoiced { get; set; }
        }
    }
}
