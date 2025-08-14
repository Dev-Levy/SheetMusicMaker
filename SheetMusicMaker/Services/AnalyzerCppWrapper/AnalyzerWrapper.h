#pragma once

using namespace System;
using namespace System::Collections::Generic;

public ref class Analyzer
{
public:
	static List<List<double>^>^ AnalyzeFile(String^ filepath, int frameSize, int hopSize);
};
