using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeScanner.WechatQRCode
{
    internal class DetectResult
    {
        [DllImport("WechatQRCodeLib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern DetectDLLResult DetectQRCodePos(byte[] pixelArray, int width, int height);
        public System.Drawing.Rectangle Points { get; set; }
        public string Result { get; set; } 
    }
}
