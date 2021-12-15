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

        private async void PasteButton_Click(object sender, RoutedEventArgs e)
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

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            PasteButton.Content = "Clicked";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(20);

            DisplayBitmap(qrCodeBitmap);
        }
    }
}
