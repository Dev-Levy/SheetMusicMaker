using System.Diagnostics;
using Models;
using Newtonsoft.Json;

namespace AnalyzerService
{
    public record PitchInterval(double PitchHz, double StartTime, double EndTime, double Duration);

    public class MusicAnalyzer
    {
        public static void Analyze(Recording recording, string outputPath)
        {
            InstallPackages();

            Console.WriteLine("Analyzing pitch! (Analyzer service)");
            Frame[] frames = AnalyzePitch(recording.Url);

            Console.WriteLine("Analyzing tempo! (Analyzer service)");
            double tempo = AnalyzeTempo(recording.Url);

            Console.WriteLine("Calculating note! (Analyzer service)");
            Array.ForEach(frames, f => f.Note = FrequencyToNoteName(f.Pitch));

            Console.WriteLine("Calculating intervals! (Analyzer service)");
            List<NoteIntervals> intervals = AggregateFrames(frames);

            Console.WriteLine();
            Console.WriteLine($"The tempo is {tempo}BPM");
            foreach (NoteIntervals interval in intervals)
            {
                if (interval.Duration > 0.1)
                {
                    Console.WriteLine($"Note: {interval.Note}");
                    Console.WriteLine($"  Start Time: {interval.StartTime:F4} s");
                    Console.WriteLine($"  End Time: {interval.EndTime:F4} s");
                    Console.WriteLine($"  Duration: {interval.Duration:F4} s");
                }
            }
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
            string pyFile = "pitch.py";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string procPyFilePath = Path.Combine(appDirectory, pyFile);

            if (!File.Exists(procPyFilePath))
            {
                throw new FileNotFoundException("Python analyzer process file not found", procPyFilePath);
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "python",
                WorkingDirectory = appDirectory,
                Arguments = $"{pyFile} \"{audioPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Console.WriteLine("Pitch analysis! (Python process call)");
            Console.WriteLine("File: " + audioPath);
            using Process process = new() { StartInfo = processInfo };
            process.Start();

            string jsonOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Frame[] frames = JsonConvert.DeserializeObject<Frame[]>(jsonOutput) ?? [];

            return frames;
        }

        private static double AnalyzeTempo(string audioPath)
        {
            string pyFile = "tempo.py";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string procPyFilePath = Path.Combine(appDirectory, pyFile);

            if (!File.Exists(procPyFilePath))
            {
                throw new FileNotFoundException("Python analyzer process file not found", procPyFilePath);
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "python",
                WorkingDirectory = appDirectory,
                Arguments = $"{pyFile} \"{audioPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Console.WriteLine("Tempo analysis! (Python process call)");
            Console.WriteLine("File: " + audioPath);
            using Process process = new() { StartInfo = processInfo };
            process.Start();

            string jsonOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            double tempo = JsonConvert.DeserializeObject<double>(jsonOutput);

            return 0;
        }

        private static List<NoteIntervals> AggregateFrames(Frame[] frames)
        {
            List<NoteIntervals> intervals = [];
            if (frames.Length == 0)
            {
                Console.WriteLine("No data to process.");
            }
            else
            {
                string currNote = frames[0].Note;
                double startTime = frames[0].Time;
                double endTime = frames[0].Time;

                for (int i = 0; i < frames.Length; i++)
                {
                    Frame frame = frames[i];

                    if (frame.Note == currNote)
                    {
                        endTime = frame.Time;
                    }
                    else
                    {
                        double duration = endTime - startTime;

                        intervals.Add(new()
                        {
                            Note = currNote,
                            StartTime = startTime,
                            EndTime = endTime,
                            Duration = duration
                        });

                        currNote = frame.Note;
                        startTime = frame.Time;
                        endTime = frame.Time;
                    }
                }

                double lastDuration = endTime - startTime;

                intervals.Add(new()
                {
                    Note = currNote,
                    StartTime = startTime,
                    EndTime = endTime,
                    Duration = lastDuration
                });
            }
            return intervals;
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

            Console.WriteLine("Installing packages! (Python process call)");
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
            [JsonIgnore]
            public string Note { get; set; }
        }

        public class NoteIntervals
        {
            public string Note { get; set; }
            public double StartTime { get; set; }
            public double EndTime { get; set; }
            public double Duration { get; set; }
        }
    }
}
