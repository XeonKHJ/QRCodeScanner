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

            _qRCodeWindow = new QRCodeWindow();
            _aboutWindow = new AboutWindow();       
            _errorDialog = new ErrorDialog();
        }

        private QRCodeWindow _qRCodeWindow;
        private AboutWindow _aboutWindow;
        private ErrorDialog _errorDialog;
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
                    _qRCodeWindow.SetQRCodeSource(bitmapImage);
                    _qRCodeWindow.XamlRoot = this.Content.XamlRoot;
                    await _qRCodeWindow.ShowAsync();
                }
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            ScanQRCodeFromPaste();
        }

        private async void ScanQRCodeFromPaste()
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
                            ContentTextBox.Text = result.Text;
                        }
                        else
                        {
                            DisplayError("No text was decoded from the image.");
                        }
                    }
                }
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            //PasteButton.Content = "Clicked";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(ContentTextBox.Text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(20);

            DisplayBitmap(qrCodeBitmap);
        }

        private void ContentTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            ScanQRCodeFromPaste();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
          
            _aboutWindow.XamlRoot = this.Content.XamlRoot;
            await _aboutWindow.ShowAsync();
        }

        private async void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = PInvoke.User32.GetActiveWindow();

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            
            if (file != null)
            {
                System.Diagnostics.Debug.WriteLine("fuck");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("shit");
            }
        }
        
        private async void DisplayError(string error)
        {
            _errorDialog.XamlRoot = this.Content.XamlRoot;
            _errorDialog.SetErrorMessage(error);
            await _errorDialog.ShowAsync();
        }
    }
}
