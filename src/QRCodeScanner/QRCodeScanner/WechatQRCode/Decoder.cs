using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeScanner.WechatQRCode
{
    internal class Decoder
    {
        [DllImport("WechatQRCodeLib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int DetectQRCodePos(int width, int height, byte[] pixelArray);

        [DllImport("WechatQRCodeLib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int LoadModel(string shit);
        public void PrepareModel()
        {
            Decoder.LoadModel("fuck you");
        }

        public void DetectAndDecode(int width, int height, byte[] pixelArray)
        {
            DetectQRCodePos(width, height, pixelArray);
        }
    }
}
