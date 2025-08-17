#include "pch.h"
#include "framework.h"

#define MINIAUDIO_IMPLEMENTATION
#include <miniaudio.h>
#include <kissfft/kiss_fft.h>
#include <kissfft/kiss_fftr.h>
#include "AnalyzerNative.hpp"

#include <string>
#include <vector>
#include <numbers>
#include <cmath>
#include <fstream>
#include <stdexcept>

#include <iostream>

struct FrameSpectrum {
	std::vector<double> magnitudes;
};

static std::vector<double> hannWindow(int size) {
	std::vector<double> window(size);
	for (int n = 0; n < size; n++)
		window[n] = 0.5 - 0.5 * cos((2.0 * std::numbers::pi * n) / (size - 1));
	return window;
}

static std::vector<FrameSpectrum> DoAnalysis(const std::string& filepath, int frameSize, int hopSize)
{
	// Load audio samples
	ma_decoder decoder;
	if (ma_decoder_init_file(filepath.c_str(), NULL, &decoder) != MA_SUCCESS)
		throw std::runtime_error("Failed to load audio file");

	std::cout << "FILE LOADED!!!" << std::endl;

	ma_uint64 totalFrames = 0;
	ma_uint64 framesRead = 0;

	std::vector<float> pcm(static_cast<size_t>(totalFrames) * decoder.outputChannels);

	ma_decoder_get_length_in_pcm_frames(&decoder, &totalFrames);
	ma_result r = ma_decoder_read_pcm_frames(&decoder, pcm.data(), totalFrames, &framesRead);
	if (r != MA_SUCCESS)
		throw std::runtime_error("Failed to read PCM frames");

	ma_decoder_uninit(&decoder);

	std::cout << "SAMPLES LOADED!!!" << std::endl;

	// Convert to mono
	std::vector<double> samples;
	samples.reserve(static_cast<size_t>(framesRead));

	for (ma_uint64 f = 0; f < framesRead; ++f) {
		double sum = 0.0;
		for (int ch = 0; ch < decoder.outputChannels; ++ch)
			sum += pcm[static_cast<size_t>(f) * decoder.outputChannels + ch];
		samples.push_back(sum / decoder.outputChannels);
	}

	std::cout << "MONO CONVERSION DONE!!!" << std::endl;

	// Framing + Windowing
	auto window = hannWindow(frameSize);
	std::vector<FrameSpectrum> allFrames;

	kiss_fft_cfg cfg = kiss_fft_alloc(frameSize, 0, NULL, NULL);

	for (size_t start = 0; start + frameSize <= samples.size(); start += hopSize) {
		std::vector<kiss_fft_cpx> in(frameSize);
		std::vector<kiss_fft_cpx> out(frameSize);

		for (int n = 0; n < frameSize; n++) {
			in[n].r = static_cast<float>(samples[start + n] * window[n]);
			in[n].i = 0.0;
		}

		// FFT
		kiss_fft(cfg, in.data(), out.data());

		// Convert to magnitude spectrum
		FrameSpectrum spectrum;
		spectrum.magnitudes.resize(frameSize / 2);
		for (int k = 0; k < frameSize / 2; k++) {
			spectrum.magnitudes[k] = std::sqrt(out[k].r * out[k].r + out[k].i * out[k].i);
		}

		allFrames.push_back(std::move(spectrum));
	}

	free(cfg);
	return allFrames;
}

int __cdecl AnalyzeAudioFile(const char* inputPath, int frameSize, int hopSize, const char* outputPath)
{
	try {
		if (!inputPath || !outputPath) return 3;						// null pointer
		if (frameSize <= 0 || (frameSize & (frameSize - 1))) return 4;	// require power-of-two
		if (hopSize <= 0 || hopSize > frameSize) return 5;

		auto frames = DoAnalysis(inputPath, frameSize, hopSize);

		std::ofstream out(outputPath);
		if (!out.is_open()) {
			return 2;
		}

		for (size_t i = 0; i < frames.size(); i++) {

			for (size_t j = 0; j < frames[i].magnitudes.size(); j++) {
				out << frames[i].magnitudes[j];

				if (j + 1 < frames[i].magnitudes.size())
					out << ";";
			}
			out << std::endl;
		}

		return 0;
	}
	catch (const std::exception& ex) {
		std::ofstream log("AnalyzerNative.log", std::ios::app);
		log << "Exception: " << ex.what() << std::endl;
		return 1;
	}
	catch (...) {
		std::ofstream log("AnalyzerNative.log", std::ios::app);
		log << "Unknown exception" << std::endl;
		return 1;
	}
}
