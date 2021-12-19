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
        public async void PrepareModel(string basePath)
        {
            await Task.Run(() =>
            {
                var result = Decoder.LoadModel(basePath);
                if (result == 0)
                {
                    IsDecoderLoaded = true;
                }
            });

        }

        public bool IsDecoderLoaded { private set; get; }
        public bool IsScanning { private set; get; }
        public async Task<string> DetectAndDecodeAsync(int width, int height, byte[] pixelArray, int channel)
        {
            IsScanning = true;
            IntPtr resultPtr = IntPtr.Zero;
            string strRet = string.Empty;
            int length = 0;
            if (IsDecoderLoaded)
            {
                await Task.Run(() =>
                {
                    length = DetectQRCodePos(width, height, pixelArray, channel, ref resultPtr);
                    strRet = Marshal.PtrToStringUTF8(resultPtr);
                    FreeResultString(resultPtr, length);
                });
            }
            else
            {
                throw new Exception("Decoder is not ready.");
            }
            IsScanning = false;

            return strRet;
        }
    }
}
