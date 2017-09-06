#include "stdafx.h"
#include "KirkinMath.h"

extern "C" __declspec(dllexport) int Add(int a, int b)
{
    return a + b;
}