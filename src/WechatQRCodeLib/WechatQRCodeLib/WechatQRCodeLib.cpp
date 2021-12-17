#include "pch.h"
#include "WechatQRCodeLib.h"
#include <opencv2/core.hpp>
#include "wechat_qrcode/wechat_qrcode.hpp"

WECHATQRCODELIB_API int ReturnSameInt(int a)
{
	return a + 1;
}

WECHATQRCODELIB_API DetectResult DetectQRCodePos(BYTE[] pixelArray, int width, int height, PixelFormat format)
{
	auto detector = cv::wechat_qrcode::WeChatQRCode("detect.prototxt", "detect.caffemodel", "sr.prototxt", "sr.caffemodel");
	auto img = cv::Mat(width, height, CV_32F);
	img.data = pixelArray;
	auto result = detector.detectAndDecode(img);
	DetectResult detectResult;
	if (result.size() != 0)
	{
		detectResult.result = result[0];
	}

	return detectResult;
}


