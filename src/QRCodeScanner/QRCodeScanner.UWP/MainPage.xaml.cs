using QRCoder;
using QRCoder.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
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

            _qRCodeWindow = new QRCodeDialog();
            _aboutWindow = new AboutDialog();
            _errorDialog = new ErrorDialog();

            frameTimer = new DispatcherTimer();
            frameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 20);
            frameTimer.Tick += FrameTimer_Tick;
        }


        private WechatQRCode.Decoder _decoder;
        private QRCodeDialog _qRCodeWindow;
        private AboutDialog _aboutWindow;
        private ErrorDialog _errorDialog;

        public async void DisplayBitmap(BitmapImage bitmapImage)
        {
            if (bitmapImage != null)
            {
                _qRCodeWindow.SetQRCodeSource(bitmapImage);
                await _qRCodeWindow.ShowAsync();
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
                        try
                        {
                            await ScanQRCodeFromStream(imageStream);
                        }
                        catch (Exception ex)
                        {
                            DisplayError(ex.Message);
                        }
                    }
                }
            }
        }

        private async Task ScanQRCodeFromStream(IRandomAccessStream stream, bool IsErrorShown = true)
        {
            BitmapDecoder dec = await BitmapDecoder.CreateAsync(stream);
            var data = await dec.GetPixelDataAsync();
            var bytes = data.DetachPixelData();

            int bytePerPixel = 4;
            switch (dec.BitmapPixelFormat)
            {
                case BitmapPixelFormat.Gray8:
                    bytePerPixel = 1;

                    break;
                case BitmapPixelFormat.Unknown:
                    bytePerPixel = 3;

                    break;
                case BitmapPixelFormat.Rgba8:
                case BitmapPixelFormat.Bgra8:
                    bytePerPixel = 4;
                    break;
                default:
                    throw new Exception(String.Format("Image format {0} is not supported.", dec.BitmapPixelFormat));
            }



            var result = await _decoder.DetectAndDecodeAsync((int)dec.PixelWidth, (int)dec.PixelHeight, bytes, bytePerPixel).ConfigureAwait(true);

            // do something with the result
            if (result != null)
            {
                await StopCamera();
                ContentTextBox.Text = result;
            }
            else
            {
                if (IsErrorShown)
                {
                    DisplayError("No text was decoded from the image.");
                }
                System.Diagnostics.Debug.WriteLine("No text.");

            }
        }

        private async Task ScanQRCodeFromStream(byte[] pixelArray, int width, int height, int bytesPerPixel, bool IsErrorShown = true)
        {

            var result = await _decoder.DetectAndDecodeAsync((int)width, (int)height, pixelArray, bytesPerPixel).ConfigureAwait(true);

            // do something with the result
            if (result != String.Empty)
            {
                await StopCamera();
                ContentTextBox.Text = result;
            }
            else
            {
                if (IsErrorShown)
                {
                    DisplayError("No text was decoded from the image.");
                }
                System.Diagnostics.Debug.WriteLine("No text.");

            }
        }
        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            //PasteButton.Content = "Clicked";

            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(ContentTextBox.Text, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCodeBmp = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeImageBmp = qrCodeBmp.GetGraphic(20, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 });

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(qrCodeImageBmp);
                        await writer.StoreAsync();
                    }
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream);

                    DisplayBitmap(image);
                }
            }
            catch (DataTooLongException ex)
            {
                DisplayError(App.ResourceLoader.GetString("DataTooLongError"));
            }
            catch(Exception ex)
            {
                DisplayError(ex.Message);
            }

        }

        private void ContentTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            ScanQRCodeFromPaste();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            await _aboutWindow.ShowAsync();
        }

        private async void OpenImageButton_Click(object sender, RoutedEventArgs e)
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
                using (IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    try
                    {
                        await ScanQRCodeFromStream(stream);
                    }
                    catch (ArgumentException ex)
                    {
                        DisplayError("File error.");
                    }
                    catch (Exception ex)
                    {
                        DisplayError(ex.Message);
                    }
                }
            }
        }


        private async void DisplayError(string error)
        {
            _errorDialog.SetErrorMessage(error);
            await _errorDialog.ShowAsync();
        }

        private async void ContentTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    using (IRandomAccessStream stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        try
                        {
                            await ScanQRCodeFromStream(stream);
                        }
                        catch (ArgumentException ex)
                        {
                            DisplayError("File error.");
                        }
                        catch (Exception ex)
                        {
                            DisplayError(ex.Message);
                        }
                    }
                }
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        DispatcherTimer frameTimer = new DispatcherTimer();
        //OpenCvSharp.VideoCapture m_vCapture;
        private bool isCameraOn = false;

        ObservableCollection<CameraDeviceViewModel> cameraDevices = new System.Collections.ObjectModel.ObservableCollection<CameraDeviceViewModel>();
        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isCameraOn)
                {
                    var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                    var groups = await MediaFrameSourceGroup.FindAllAsync();

                    // Filter out color video preview and video record type sources and remove duplicates video devices.
                    var cameraList = groups.Where(g => g.SourceInfos.Any(s => s.SourceKind == MediaFrameSourceKind.Color &&
                                                                                (s.MediaStreamType == MediaStreamType.VideoPreview || s.MediaStreamType == MediaStreamType.VideoRecord))
                                                                                && g.SourceInfos.All(sourceInfo => videoDevices.Any(vd => vd.Id == sourceInfo.DeviceInformation.Id))).ToList();

                    cameraDevices.Clear();
                    CameraListFlyout.Items.Clear();
                    for (int i = 0; i < cameraList.Count; ++i)
                    {
                        var deviceViewModel = new CameraDeviceViewModel
                        {
                            Id = cameraList[i].Id,
                            Name = cameraList[i].DisplayName
                        };
                        cameraDevices.Add(deviceViewModel);

                        var flyoutItem = new MenuFlyoutItem
                        {
                            Text = cameraList[i].DisplayName,
                            Tag = deviceViewModel
                        };
                        flyoutItem.Click += FlyoutItem_Click;
                        CameraListFlyout.Items.Add(flyoutItem);
                    }

                    if (CameraListFlyout.Items.Count > 0)
                    {
                        var firstItem = CameraListFlyout.Items[0].Tag as CameraDeviceViewModel;
                        StartCamera(firstItem);
                    }
                    else
                    {
                        DisplayError(App.ResourceLoader.GetString("NoCameraError"));
                    }

                }
                else
                {
                    await StopCamera();
                }
            }
            catch (Exception ex)
            {
                DisplayError("Camera failed.");
                //StopCamera();
            }

        }

        private async void FlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!isClosing)
            {
                var flyoutItem = (MenuFlyoutItem)sender;
                var viewModel = flyoutItem.Tag as CameraDeviceViewModel;
                if (viewModel != null)
                {
                    await StopCamera(false);
                    StartCamera(viewModel);
                }
            }

        }

        private MediaCapture mediaCapture;
        private DisplayRequest displayRequest;
        async void StartCamera(CameraDeviceViewModel viewModel)
        {
            //try
            //{
            //    m_vCapture = new OpenCvSharp.VideoCapture(viewModel.Id);

            //    if (!m_vCapture.IsOpened())
            //    {
            //        var displayErrorMsg = App.ResourceLoader.GetString("CameraPermissionError");
            //        DisplayError(displayErrorMsg);
            //        return;
            //    }
            //    else
            //    {
            //        CameraListDropDownButton.Content = viewModel.Name;

            //        isCameraOn = true;
            //        //m_vCapture.Set(OpenCvSharp.VideoCaptureProperties.FrameWidth, 100);//宽度
            //        //m_vCapture.Set(OpenCvSharp.VideoCaptureProperties.FrameHeight, 100);//高度
            //        CameraPreviewGrid.Visibility = Visibility.Visible;
            //        ContentTextBox.Visibility = Visibility.Collapsed;
            //        DescriptionTextBlock.Visibility = Visibility.Collapsed;
            //        frameTimer.Start();
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            displayRequest = new DisplayRequest();
            try
            {

                mediaCapture = new MediaCapture();
                var mediaInitSettings = new MediaCaptureInitializationSettings()
                {
                    VideoDeviceId = viewModel.Id
                };
                await mediaCapture.InitializeAsync(mediaInitSettings);

                displayRequest.RequestActive();
            }
            catch (UnauthorizedAccessException)
            {
                var displayErrorMsg = App.ResourceLoader.GetString("CameraPermissionError");
                // This will be thrown if the user denied access to the camera in privacy settings
                DisplayError(displayErrorMsg);
                return;
            }
            try
            {
                CamerePreviewElement.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                CameraListDropDownButton.Content = viewModel.Name;
                isCameraOn = true;

                CameraPreviewGrid.Visibility = Visibility.Visible;
                ContentTextBox.Visibility = Visibility.Collapsed;
                DescriptionTextBlock.Visibility = Visibility.Collapsed;
                frameTimer.Start();
            }
            catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += MediaCapture_CaptureDeviceExclusiveControlStatusChanged; ;
            }
        }

        private async void MediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                DisplayError("The camera preview can't be displayed because another app has exclusive access");
                frameTimer.Stop();
                await StopCamera();

            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isCameraOn)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    StartCamera(cameraDevices[0]);
                });
            }
        }

        bool isClosing = false;
        async Task StopCamera(bool isUIChange = true)
        {
            isClosing = true;
            //isCameraOn = false;
            //frameTimer.Stop();

            //if (m_vCapture != null)
            //{
            //    m_vCapture.Dispose();
            //}

            //m_vCapture = null;
            //CameraPreviewGrid.Visibility = Visibility.Collapsed;
            //ContentTextBox.Visibility = Visibility.Visible;
            //DescriptionTextBlock.Visibility = Visibility.Visible;

            if (mediaCapture != null)
            {
                if (isCameraOn)
                {
                    await mediaCapture.StopPreviewAsync();
                    isCameraOn = false;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CamerePreviewElement.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;

                    if (isUIChange)
                    {
                        CameraPreviewGrid.Visibility = Visibility.Collapsed;
                        ContentTextBox.Visibility = Visibility.Visible;
                        DescriptionTextBlock.Visibility = Visibility.Visible;
                    }
                });
            }
            isClosing = false;
        }

        VideoEncodingProperties previewProperties;
        private async void FrameTimer_Tick(object sender, object e)
        {
            try
            {
                if (mediaCapture != null)
                {
                    if (!_decoder.IsScanning)
                    {
                        VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
                        VideoFrame previewFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame);

                        if (previewFrame != null)
                        {
                            SoftwareBitmap previewBitmap = previewFrame.SoftwareBitmap;
                            byte[] data = new byte[(int)previewProperties.Width * (int)previewProperties.Height * 4];
                            previewBitmap.CopyToBuffer(data.AsBuffer());
                            await ScanQRCodeFromStream(data, (int)previewProperties.Width, (int)previewProperties.Height, 4, false);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        //private async void FrameTimer_Tick(object sender, object e)
        //{
        //    if (m_vCapture != null)
        //    {
        //        //Thread.Sleep(40);
        //        OpenCvSharp.Mat cFrame = new OpenCvSharp.Mat();
        //        try
        //        {
        //            m_vCapture.Read(cFrame);

        //            var imageBytes = cFrame.ToBytes();
        //            var displayBytes = new byte[imageBytes.Length];
        //            imageBytes.CopyTo(displayBytes, 0);
        //            //using (MemoryStream memoryStream = new MemoryStream(imageBytes))
        //            //{
        //            using (MemoryStream memoryStream = new MemoryStream(imageBytes))

        //            using (var imageStream = imageBytes.AsBuffer().AsStream())
        //            using (var displayStream = displayBytes.AsBuffer().AsStream())
        //            {
        //                System.Drawing.Bitmap bitmap = new Bitmap(memoryStream);
        //                BitmapImage bitmapImage = new BitmapImage();

        //                var raStream = displayStream.AsRandomAccessStream();
        //                await bitmapImage.SetSourceAsync(raStream);
        //                CameraPreviewImage.Source = bitmapImage;
        //                if (!_decoder.IsScanning)
        //                {
        //                    ScanQRCodeFromStream(imageStream, false);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex.Message == "!image.empty()")
        //            {
        //                if (isCameraOn)
        //                {
        //                    //StopCamera();
        //                    frameTimer.Stop();
        //                    DisplayError(App.ResourceLoader.GetString("CameraIsOccupiedError"));
        //                }
        //            }
        //            else
        //            {
        //                DisplayError(ex.Message);
        //            }
        //            System.Diagnostics.Debug.WriteLine("dskfjlsd");
        //        }

        //        cFrame.Release();
        //        cFrame.Dispose();
        //    }

        //}

        //private async void ScanQRCodeFromStream(IRandomAccessStream stream, bool IsErrorShown = true)
        //{

        //    BitmapDecoder dec = await BitmapDecoder.CreateAsync(stream);
        //    var data = await dec.GetPixelDataAsync();
        //    var bytes = data.DetachPixelData();
        //    //var pixel = GetPixel(bytes, 1, 1, dec.PixelWidth, dec.PixelHeight);



        //    var result = await _decoder.DetectAndDecodeAsync((int)dec.PixelWidth, (int)dec.PixelHeight, bytes, 4).ConfigureAwait(true);

        //    // do something with the result
        //    //if (result != null)
        //    //{
        //    //    StopCamera();
        //    //    ContentTextBox.Text = result;
        //    //}
        //    //else
        //    //{
        //    //    if (IsErrorShown)
        //    //    {
        //    //        DisplayError("No text was decoded from the image.");
        //    //    }
        //    //    System.Diagnostics.Debug.WriteLine("No text.");

        //    //}
        //}
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
                        await ScanQRCodeFromStream(stream.AsRandomAccessStream());
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
