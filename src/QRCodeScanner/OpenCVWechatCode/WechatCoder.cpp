#include "pch.h"
#include "WechatCoder.h"
#include "WechatCoder.g.cpp"

#include <opencv2/core.hpp>

#include <opencv2/imgproc.hpp>
#include <opencv2/opencv.hpp>

#include <string>
using std::string;

namespace winrt::OpenCVWechatCode::implementation
{
	int32_t WechatCoder::LoadModel(hstring const& basePath)
	{
		string basePathSlash = winrt::to_string(basePath) + "\\";

		detector = cv::wechat_qrcode::WeChatQRCode(basePathSlash + "detect.prototxt", basePathSlash + "detect.caffemodel", basePathSlash + "sr.prototxt", basePathSlash + "sr.caffemodel");
		return 0;
	}
	float WechatCoder::GetNumber()
	{
		throw hresult_not_implemented();
	}
	hstring WechatCoder::Decode(int32_t width, int32_t height, array_view<uint8_t const> pixelArray, int32_t channel)
	{
		int type = CV_8UC4;
		switch (channel)
		{
		case 4:
			type = CV_8UC4;
			break;
		case 3:
			type = CV_8UC3;
			break;
		case 1:
			type = CV_8UC1;
			break;
		default:
			break;
		}
		auto img = cv::Mat(height, width, type);
		uint8_t* dataArray = new uint8_t[pixelArray.size()];
		memcpy(dataArray, pixelArray.begin(), pixelArray.size());
		img.data = dataArray;

		auto results = detector.detectAndDecode(img);
		int returnSize = 0;
		char* resultchararray = nullptr;
		hstring result;
		if (results.size() != 0)
		{
			auto resultString = results[0];
			result = winrt::to_hstring(resultString);
		}
		else
		{
			// No result
		}
		delete[] dataArray;
		img.release();

		return result;
	}
}
