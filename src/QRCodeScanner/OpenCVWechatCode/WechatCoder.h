#pragma once
#include "WechatCoder.g.h"

#include "../../WechatQRCodeLib/WechatQRCodeLib/wechat_qrcode/wechat_qrcode.hpp"

namespace winrt::OpenCVWechatCode::implementation
{
    struct WechatCoder : WechatCoderT<WechatCoder>
    {
        WechatCoder() = default;

        int32_t LoadModel(hstring const& basePath);
        float GetNumber();
        hstring Decode(int32_t width, int32_t height, array_view<uint8_t const> pixelArray, int32_t channel);

    private:
        cv::wechat_qrcode::WeChatQRCode detector;
    };
}
namespace winrt::OpenCVWechatCode::factory_implementation
{
    struct WechatCoder : WechatCoderT<WechatCoder, implementation::WechatCoder>
    {
    };
}
