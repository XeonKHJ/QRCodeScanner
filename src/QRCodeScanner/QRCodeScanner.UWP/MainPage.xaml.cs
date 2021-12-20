using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace QRCodeScanner.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            _decoder = new WechatQRCode.Decoder();
            _decoder.PrepareModel(installedLocation + "\\MLModels");
        }

        WechatQRCode.Decoder _decoder;
        private async void ScanQRCodeFromStream(IRandomAccessStream stream, bool IsErrorShown = true)
        {

            BitmapDecoder dec = await BitmapDecoder.CreateAsync(stream);
            var data = await dec.GetPixelDataAsync();
            var bytes = data.DetachPixelData();
            //var pixel = GetPixel(bytes, 1, 1, dec.PixelWidth, dec.PixelHeight);



            var result = await _decoder.DetectAndDecodeAsync((int)dec.PixelWidth, (int)dec.PixelHeight, bytes, 4).ConfigureAwait(true);

            // do something with the result
            //if (result != null)
            //{
            //    StopCamera();
            //    ContentTextBox.Text = result;
            //}
            //else
            //{
            //    if (IsErrorShown)
            //    {
            //        DisplayError("No text was decoded from the image.");
            //    }
            //    System.Diagnostics.Debug.WriteLine("No text.");

            //}
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    try
                    {


                        //var byteArray = new byte[readStream.Length];
                        //await readStream.ReadAsync(byteArray, 0, byteArray.Length);

                        ScanQRCodeFromStream(stream.AsRandomAccessStream());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
