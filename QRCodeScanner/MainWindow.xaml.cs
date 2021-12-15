using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ZXing;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QRCodeScanner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            pasteButton.Content = "Clicked";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(20);

            DisplayBitmap(qrCodeBitmap);
        }
        public async void DisplayBitmap(Bitmap bitmap)
        {
            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                var systemStream = stream.AsStreamForWrite();
                BitmapImage bitmapImage = new BitmapImage();
                await Task.Run(() =>
                {
                    bitmap.Save(systemStream, ImageFormat.Bmp);
                    stream.Seek(0);
                    
                }).ConfigureAwait(true);
                await bitmapImage.SetSourceAsync(stream);
                if (bitmapImage!=null)
                {
                    qrCodeImage.Source = bitmapImage;
                }
            }
        }
        private void useCameraButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Fuckoff");
        }

        private void TextBox_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("shit");
        }

        private void TextBox_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("fuck");
        }

        private async void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                Windows.Storage.Streams.IRandomAccessStreamReference imageReceived = null;
                imageReceived = await dataPackageView.GetBitmapAsync();
                if (imageReceived != null)
                {
                    using (var imageStream = await imageReceived.OpenReadAsync())
                    {
                        Bitmap bitmap = (Bitmap)System.Drawing.Image.FromStream(imageStream.AsStream());
                        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                        var length = bitmapData.Width * bitmapData.Height * 4;
                        byte[] bytes = new byte[length];

                        // Copy bitmap to byte[]
                        Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
                        bitmap.UnlockBits(bitmapData);

                        //imageStream.Seek(0);
                        //Windows.Storage.Streams.Buffer buffer = new((uint)imageStream.Size);
                        //await imageStream.ReadAsync(buffer, (uint)imageStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                        //byte[] imageBytes = new byte[buffer.Length];
                        //Windows.Storage.Streams.DataReader.FromBuffer(buffer).ReadBytes(imageBytes);

                        byte[] abcd = new byte[bitmap.Width * bitmap.Height * 4 + 10];
                        //GetRGB(bitmap, 0, 0, bitmap.Width, bitmap.Height, abcd, 0, bitmap.Width);

                        /// Scan
                        // create a barcode reader instance
                        IBarcodeReader reader = new BarcodeReader();
                        // detect and decode the barcode inside the bitmap
                        var result = reader.Decode(bytes, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.RGB32);
                        // do something with the result
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("fuck");
                        }
                    }
                }
            }
            else
            {
                //tblStatus.Text = "Status : Bitmap format is not available in clipboard";
            }
        }

        //public static void GetRGB(this Bitmap image, int startX, int startY, int w, int h, int[] rgbArray, int offset, int scansize)
        //{
        //    const int PixelWidth = 4;
        //    const PixelFormat PixelFormat = PixelFormat.Format32bppRgb;

        //    // En garde!
        //    if (image == null) throw new ArgumentNullException("image");
        //    if (rgbArray == null) throw new ArgumentNullException("rgbArray");
        //    if (startX < 0 || startX + w > image.Width) throw new ArgumentOutOfRangeException("startX");
        //    if (startY < 0 || startY + h > image.Height) throw new ArgumentOutOfRangeException("startY");
        //    if (w < 0 || w > scansize || w > image.Width) throw new ArgumentOutOfRangeException("w");
        //    if (h < 0 || (rgbArray.Length < offset + h * scansize) || h > image.Height) throw new ArgumentOutOfRangeException("h");

        //    BitmapData data = image.LockBits(new Rectangle(startX, startY, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat);
        //    try
        //    {
        //        byte[] pixelData = new Byte[data.Stride];
        //        for (int scanline = 0; scanline < data.Height; scanline++)
        //        {
        //            Marshal.Copy(data.Scan0 + (scanline * data.Stride), pixelData, 0, data.Stride);
        //            for (int pixeloffset = 0; pixeloffset < data.Width; pixeloffset++)
        //            {
        //                // PixelFormat.Format32bppRgb means the data is stored
        //                // in memory as BGR. We want RGB, so we must do some 
        //                // bit-shuffling.
        //                rgbArray[offset + (scanline * scansize) + pixeloffset] =
        //                    (pixelData[pixeloffset * PixelWidth + 2] << 16) +   // R 
        //                    (pixelData[pixeloffset * PixelWidth + 1] << 8) +    // G
        //                    pixelData[pixeloffset * PixelWidth];                // B
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        image.UnlockBits(data);
        //    }
        //}

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            pasteButton.Content = "Clicked";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(20);

            DisplayBitmap(qrCodeBitmap);
        }
    }
}
