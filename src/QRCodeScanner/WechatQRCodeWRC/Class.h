#pragma once

#include "Class.g.h"

namespace winrt::WechatQRCodeWRC::implementation
{
    struct Class : ClassT<Class>
    {
        Class() = default;

        int32_t MyProperty();
        void MyProperty(int32_t value);
    };
}

namespace winrt::WechatQRCodeWRC::factory_implementation
{
    struct Class : ClassT<Class, implementation::Class>
    {
    };
}
