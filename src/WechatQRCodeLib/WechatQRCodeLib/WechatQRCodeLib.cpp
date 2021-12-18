#include "pch.h"
#include "WechatQRCodeLib.h"
#include <opencv2/core.hpp>
#include "wechat_qrcode/wechat_qrcode.hpp"

bool isInit = false;
cv::wechat_qrcode::WeChatQRCode * detector;

WECHATQRCODELIB_API int ReturnSameInt(int a)
{
	return a + 1;
}

WECHATQRCODELIB_API int DetectQRCodePos( int width, int height, uchar* pixelArray)
{
	auto img = cv::Mat(height, width, CV_8U);
	img.data = pixelArray;
	auto pixelCount = width * height;
	auto rArray = new uchar[pixelCount];
	auto gArray = new uchar[pixelCount];
	auto bArray = new uchar[pixelCount];

	int channel = 4;
	int offset = 0;
	int i = 0;
	for (offset = 0, i = 0; offset < pixelCount * channel; offset += channel, ++i)
	{
		rArray[i] = pixelArray[offset + 1];
		gArray[i] = pixelArray[offset + 2];
		bArray[i] = pixelArray[offset + 3];
	}
	auto rImg = cv::Mat(height, width, CV_8U);
	rImg.data = rArray;
	auto gImg = cv::Mat(height, width, CV_8U);
	gImg.data = gArray;
	auto bImg = cv::Mat(height, width, CV_8U);
	bImg.data = bArray;
	cv::Mat matChannels[3] = { rImg, gImg, bImg };
	cv::Mat mergeImg;
	cv::merge(matChannels, 3, mergeImg);
	auto channels = mergeImg.channels();

	auto result = detector->detectAndDecode(mergeImg);
	
	DetectResult detectResult;
	if (result.size() != 0)
	{
		detectResult.result = result[0];
	}

	delete[] rArray;
	delete[] gArray;
	delete[] bArray;

	return 1;
}

WECHATQRCODELIB_API int LoadModel(char * shit)
{
	string baseshit = string(shit);
	std::string basePath = "C:\\Users\\redal\\source\\repos\\QRCodeScanner\\DLModel\\";
	detector = new cv::wechat_qrcode::WeChatQRCode(basePath + "detect.prototxt", basePath + "detect.caffemodel", basePath+"sr.prototxt", basePath+"sr.caffemodel");
	isInit = true;
	return 0;
}



