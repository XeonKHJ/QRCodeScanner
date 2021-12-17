#pragma once
#ifdef WECHATQRCODELIB_EXPORTS
#define WECHATQRCODELIB_API __declspec(dllexport)
#else
#define WECHATQRCODELIB_API __declspec(dllimport)
#endif

extern "C" WECHATQRCODELIB_API void ReturnSameInt(int a);

extern "C" WECHATQRCODELIB_API void DetectQRCodePos();

extern "C" WECHATQRCODELIB_API void ScanQRCode();

