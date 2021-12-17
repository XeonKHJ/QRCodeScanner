#pragma once
#ifdef WECHATQRCODELIB_EXPORTS
#define WECHATQRCODELIB_API __declspec(dllexport)
#else
#define WECHATQRCODELIB_API __declspec(dllimport)
#endif

#include <string>
using std::string;

enum PixelFormat {
	ARGB32,
	RGBA32
};

struct DetectResult {
	int resultCount;
	//int* widths;
	//int* heights;
	string result;
};

extern "C" WECHATQRCODELIB_API int ReturnSameInt(int a);

extern "C" WECHATQRCODELIB_API DetectResult DetectQRCodePos(BYTE * pixelArray, int width, int height, PixelFormat format);