// AnalyzerNative.hpp
#pragma once

#ifdef ANALYZERNATIVE_EXPORTS
#  define ANALYZERNATIVE_API __declspec(dllexport)
#else
#  define ANALYZERNATIVE_API __declspec(dllimport)
#endif

extern "C" ANALYZERNATIVE_API int __cdecl AnalyzeAudioFile(
	const char* inputPath,
	int frameSize,
	int hopSize,
	const char* outputPath);
