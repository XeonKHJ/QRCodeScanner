#pragma once

#include "Class.g.h"

namespace winrt::WechatQRCodeLib_WRC::implementation
{
    struct Class : ClassT<Class>
    {
        Class() = default;

        int32_t MyProperty();
        void MyProperty(int32_t value);
    };
}

namespace winrt::WechatQRCodeLib_WRC::factory_implementation
{
    struct Class : ClassT<Class, implementation::Class>
    {
    };
}
