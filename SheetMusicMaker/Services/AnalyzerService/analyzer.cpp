#include "pch.h"

#include <cmath>

extern "C" __declspec(dllexport) double AddNumbers(double a, double b)
{
	return a + b;
}

extern "C" __declspec(dllexport) double Hypotenuse(double a, double b)
{
	return std::sqrt(a * a + b * b);
}
