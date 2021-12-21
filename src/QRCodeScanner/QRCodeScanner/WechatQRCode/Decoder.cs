using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WechatQRCodeWRC;

namespace QRCodeScanner.WechatQRCode
{
    internal class Decoder
    {
        private WechatQRDecoder decoder;
        public Decoder()
        {
            decoder = new WechatQRDecoder();
        }
        public async void PrepareModel(string basePath)
        {
            await Task.Run(() =>
            {
                decoder.LoadModel(basePath);
            });
            IsDecoderLoaded = true;

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
                    strRet = decoder.Decode(width, height, pixelArray, channel);
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
