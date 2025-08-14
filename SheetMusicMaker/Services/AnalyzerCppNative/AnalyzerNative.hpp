#pragma once
#include <vector>
#include <string>

struct FrameSpectrum {
	std::vector<double> magnitudes; // frequency bins for one frame
};

std::vector<FrameSpectrum> AnalyzeAudioFile(const std::string& filepath, int frameSize = 1024, int hopSize = 512);
