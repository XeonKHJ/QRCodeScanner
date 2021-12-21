#pragma once
#include "WechatQRDecoder.g.h"
#include "wechat_qrcode/wechat_qrcode.hpp"


namespace winrt::WechatQRCodeWRC::implementation
{
    struct WechatQRDecoder : WechatQRDecoderT<WechatQRDecoder>
    {
        WechatQRDecoder() = default;

        int32_t LoadModel(hstring const& basePath);
        hstring Decode(int32_t width, int32_t height, array_view<uint8_t const> pixelArray, int32_t channel);

    private:
        cv::wechat_qrcode::WeChatQRCode detector;
    };
}
namespace winrt::WechatQRCodeWRC::factory_implementation
{
    struct WechatQRDecoder : WechatQRDecoderT<WechatQRDecoder, implementation::WechatQRDecoder>
    {
    };
}
