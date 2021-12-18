#include "pch.h"
#include "WechatQRCodeLib.h"
#include <opencv2/core.hpp>
#include "wechat_qrcode/wechat_qrcode.hpp"
#include <opencv2/imgproc.hpp>

bool isInit = false;
cv::wechat_qrcode::WeChatQRCode * detector;

WECHATQRCODELIB_API int ReturnSameInt(int a)
{
	return a + 1;
}

WECHATQRCODELIB_API int DetectQRCodePos( int width, int height, unsigned char * pixelArray, int channel, char ** stringResult)
{
	int type = CV_8UC4;
	switch (channel)
	{
	case 4:
		type = CV_8UC4;
		break;
	case 3:
		type = CV_8UC3;
	case 1:
		type = CV_8UC1;
	default:
		break;
	}
	auto img = cv::Mat(height, width, type);
	img.data = pixelArray;

	auto result = detector->detectAndDecode(img);
	int returnSize = 0;
	DetectResult detectResult;
	char* resultchararray = nullptr;
	if (result.size() != 0)
	{
		// fetch first result.
		auto stringLength = result[0].length() + 1;
		returnSize = stringLength;
		resultchararray = new char[stringLength];
		memset(resultchararray, 0, stringLength);
		memcpy((void*)resultchararray, result[0].c_str(), stringLength - 1);
		*stringResult = resultchararray;
	}
	else
	{
		// No result
	}

	return returnSize;
}

WECHATQRCODELIB_API int LoadModel(char * shit)
{
	string baseshit = string(shit);
	std::string basePath = "C:\\Users\\redal\\source\\repos\\QRCodeScanner\\DLModel\\";
	detector = new cv::wechat_qrcode::WeChatQRCode(basePath + "detect.prototxt", basePath + "detect.caffemodel", basePath+"sr.prototxt", basePath+"sr.caffemodel");
	isInit = true;
	return 0;
}

WECHATQRCODELIB_API void FreeResultString(char* stringptr, int size)
{
	delete[] stringptr;
}



