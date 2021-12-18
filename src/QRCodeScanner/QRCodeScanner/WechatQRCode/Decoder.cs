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
        static extern int DetectQRCodePos(int width, int height, byte[] pixelArray, int channel, ref IntPtr resultPtr);

        [DllImport("WechatQRCodeLib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int LoadModel(string shit);

        [DllImport("WechatQRCodeLib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void FreeResultString(IntPtr stringptr, int size);
        public void PrepareModel()
        {
            Decoder.LoadModel("fuck you");
        }

        public async Task<string> DetectAndDecodeAsync(int width, int height, byte[] pixelArray)
        {
            IntPtr resultPtr = IntPtr.Zero;
            string strRet = string.Empty;
            int length = 0;
            await Task.Run(() =>
            {
                length = DetectQRCodePos(width, height, pixelArray, 4, ref resultPtr);
                strRet = Marshal.PtrToStringAnsi(resultPtr);
                FreeResultString(resultPtr, length);
            });


            return strRet;
        }
    }
}
