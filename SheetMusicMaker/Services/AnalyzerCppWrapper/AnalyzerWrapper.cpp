#include "AnalyzerWrapper.h"
#include "../AnalyzerCppNative/AnalyzerNative.hpp"
#include <msclr/marshal_cppstd.h>

List<List<double>^>^ Analyzer::AnalyzeFile(String^ filepath, int frameSize, int hopSize)
{
	std::string nativePath = msclr::interop::marshal_as<std::string>(filepath);

	auto nativeFrames = AnalyzeAudioFile(nativePath, frameSize, hopSize);

	auto result = gcnew List<List<double>^>();
	for (auto& frame : nativeFrames) {
		auto managedFrame = gcnew List<double>();
		for (auto mag : frame.magnitudes)
			managedFrame->Add(mag);
		result->Add(managedFrame);
	}
	return result;
}
