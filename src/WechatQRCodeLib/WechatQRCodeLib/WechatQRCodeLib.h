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
	int resultLength;
	string result;
};

extern "C" WECHATQRCODELIB_API int ReturnSameInt(int a);

extern "C" WECHATQRCODELIB_API int DetectQRCodePos(int width, int height, unsigned char  * pixelArray, int channel, char ** stringResult);

extern "C" WECHATQRCODELIB_API int LoadModel(char * basePathCharPtr);

extern "C" WECHATQRCODELIB_API void FreeResultString(char * stringptr, int size);