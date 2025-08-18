using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

internal static class NativeAnalyzer
{
    [DllImport("AnalyzerCppNative.dll",
        CallingConvention = CallingConvention.Cdecl,
        ExactSpelling = true)]
    private static extern int AnalyzeAudioFile(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string inputPath,
        int frameSize,
        int hopSize,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string outputPath);

    public static List<List<double>> Analyze(string inputFile, IConfiguration config)
    {
        string framesDir = config["FileStorage:FramesDir"] ?? throw new ArgumentException("FramesDir missing");
        string outputFile = Path.Combine(framesDir, DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".csv");

        int frameSize = int.Parse(config["FFT:FrameSize"] ?? throw new ArgumentException("FrameSize missing"));
        int hopSize = int.Parse(config["FFT:HopSize"] ?? throw new ArgumentException("HopSize missing"));

        string inAbs = Path.GetFullPath(inputFile);
        string outAbs = Path.GetFullPath(outputFile);

        int rc = AnalyzeAudioFile(inAbs, frameSize, hopSize, outAbs);
        if (rc != 0) throw new Exception($"Native analysis failed with code {rc} (in={inAbs}, out={outAbs})");

        return File.ReadLines(outAbs)
                   .Where(l => !string.IsNullOrWhiteSpace(l))
                   .Select(l => l.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => double.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                                 .ToList())
                   .ToList();
    }
}

