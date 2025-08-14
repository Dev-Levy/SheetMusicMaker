#include "pch.h"
#include "framework.h"

#include <miniaudio.h>
#include <kissfft/kiss_fft.h>
#include <kissfft/kiss_fftr.h>

#include "AnalyzerNative.hpp"

#include <numbers>
#include <cmath>
#include <stdexcept>

static std::vector<double> hannWindow(int size) {
	std::vector<double> window(size);
	for (int n = 0; n < size; n++)
		window[n] = 0.5 - 0.5 * cos((2.0 * std::numbers::pi * n) / (size - 1));
	return window;
}

std::vector<FrameSpectrum> AnalyzeAudioFile(const std::string& filepath, int frameSize, int hopSize)
{
	// Step 1: Load audio samples
	ma_decoder decoder;
	if (ma_decoder_init_file(filepath.c_str(), NULL, &decoder) != MA_SUCCESS)
		throw std::runtime_error("Failed to load audio file");

	std::vector<float> pcm;
	{
		ma_uint64 totalFrames = 0;
		ma_decoder_get_length_in_pcm_frames(&decoder, &totalFrames);
		pcm.resize(totalFrames * decoder.outputChannels);
		ma_decoder_read_pcm_frames(&decoder, pcm.data(), totalFrames, NULL);
	}
	ma_decoder_uninit(&decoder);

	// Convert to mono if needed
	std::vector<double> samples;
	samples.reserve(pcm.size() / decoder.outputChannels);
	for (size_t i = 0; i < pcm.size(); i += decoder.outputChannels) {
		double sum = 0.0;
		for (int ch = 0; ch < decoder.outputChannels; ++ch)
			sum += pcm[i + ch];
		samples.push_back(sum / decoder.outputChannels);
	}

	// Step 2: Framing + Windowing
	auto window = hannWindow(frameSize);
	std::vector<FrameSpectrum> allFrames;

	kiss_fft_cfg cfg = kiss_fft_alloc(frameSize, 0, NULL, NULL);

	for (size_t start = 0; start + frameSize <= samples.size(); start += hopSize) {
		std::vector<kiss_fft_cpx> in(frameSize);
		std::vector<kiss_fft_cpx> out(frameSize);

		for (int n = 0; n < frameSize; n++) {
			in[n].r = samples[start + n] * window[n];
			in[n].i = 0.0;
		}

		// Step 3: FFT
		kiss_fft(cfg, in.data(), out.data());

		// Step 4: Convert to magnitude spectrum
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
