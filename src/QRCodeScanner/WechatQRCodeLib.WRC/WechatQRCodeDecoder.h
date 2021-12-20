#pragma once
#include "WechatQRCodeDecoder.g.h"
#include "../../WechatQRCodeLib/WechatQRCodeLib/wechat_qrcode/wechat_qrcode.hpp"

namespace winrt::WechatQRCodeLib_WRC::implementation
{
    struct WechatQRCodeDecoder : WechatQRCodeDecoderT<WechatQRCodeDecoder>
    {
        WechatQRCodeDecoder() = default;

        int32_t LoadModel(hstring const& basePath);
        float GetNumber();
        hstring Decode(int32_t width, int32_t height, array_view<uint8_t const> pixelArray, int32_t channel);

    private:
        cv::wechat_qrcode::WeChatQRCode detector;
    };
}
namespace winrt::WechatQRCodeLib_WRC::factory_implementation
{
    struct WechatQRCodeDecoder : WechatQRCodeDecoderT<WechatQRCodeDecoder, implementation::WechatQRCodeDecoder>
    {
    };
}
