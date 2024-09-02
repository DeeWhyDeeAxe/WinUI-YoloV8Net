using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp.PixelFormats;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Utilities;
using WinUI_YoloV8_ObjectDetection.ViewModel;

namespace WinUI_YoloV8_ObjectDetection.Pages
{
    public sealed partial class MediaInferencePage : Page
    {
        private readonly bool allowDragDrop = true;

        private IPredictor yoloV8Predictor;
        private ISettingsService settingsService;
        private IPredictorService predictorService;
        private MediaInferenceViewModel mediaInferenceViewModel;
        private SoftwareBitmap softwareBitmapImage;

        private MediaPlayer mediaPlayer;
        public MediaInferencePage()
        {
            this.InitializeComponent();

            settingsService = App.Current.ServiceProvider.GetService<ISettingsService>();
            predictorService = App.Current.ServiceProvider.GetRequiredService<IPredictorService>();
            settingsService.ApplySettings();

            mediaInferenceViewModel = new MediaInferenceViewModel();
            DataContext = mediaInferenceViewModel;

            if (allowDragDrop)
            {
                MediaInferenceGrid.AllowDrop = true;
                MediaInferenceGrid.DragOver += MediaInferenceGrid_DragOver;
            }
        }

        private async void MediaInferenceGrid_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    if (storageFile.ContentType.Equals(MediaTypeNames.Image.Jpeg) || storageFile.ContentType.Equals(MediaTypeNames.Image.Png))
                    {
                        await DetectImageFileAsync(storageFile);
                    }
                }
            }
        }

        private async void FilePicker_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            var window = MainWindow.Instance;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");
            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                if (file.ContentType.Equals(MediaTypeNames.Image.Jpeg) || file.ContentType.Equals(MediaTypeNames.Image.Png))
                {
                    await DetectImageFileAsync(file);
                }
                else if (file.ContentType.Equals("video/mp4"))
                {
                    DetectVideoFile(file);
                }
            }
        }

        private async Task DetectImageFileAsync(StorageFile storageFile)
        {
            SetLoadingState(true);
            var image = SixLabors.ImageSharp.Image.Load(storageFile.Path);
            await DisplayImageAsync(storageFile);
            await PerformDetectionAsync(image);
        }

        private void DetectVideoFile(StorageFile videoFile)
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromStorageFile(videoFile);
            mediaPlayer.IsVideoFrameServerEnabled = true;
            mediaPlayer.VideoFrameAvailable += MediaPlayer_VideoFrameAvailable;
            OriginalVideoMediaPlayerElement.SetMediaPlayer(mediaPlayer);
            mediaPlayer.Play();
        }

        private async void MediaPlayer_VideoFrameAvailable(MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SetLoadingState(true);
            });

            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 1280, 820, BitmapAlphaMode.Ignore);
            using (CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap))
            {
                sender.CopyFrameToVideoSurface(canvasBitmap);
                softwareBitmapImage = await SoftwareBitmap.CreateCopyFromSurfaceAsync(canvasBitmap);

                if (softwareBitmapImage.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmapImage.BitmapAlphaMode == BitmapAlphaMode.Straight)
                    softwareBitmapImage = SoftwareBitmap.Convert(softwareBitmapImage, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                DispatcherQueue.TryEnqueue(async () =>
                {
                    var source = new SoftwareBitmapSource();
                    await source.SetBitmapAsync(softwareBitmapImage);
                    OriginalImage.Source = source;
                });
            }
            var imageSharpImage = ImageHelpers.LoadImageSharpImage<Rgba32>(softwareBitmapImage);

            var outputImage = await predictorService.YoloPredictor.PerformDetectionAsync(imageSharpImage);
            var imageArray = ImageHelpers.ImageToImageArray(outputImage);
            DispatcherQueue.TryEnqueue(async () =>
            {
                InferenceImage.Source = await ImageHelpers.ImageArrayToBitmapImage(imageArray);
                SetLoadingState(false);
            });
        }

        private void SetLoadingState(bool loading)
        {
            mediaInferenceViewModel.ReferenceGuideVisible = Visibility.Collapsed;
            if (loading)
                mediaInferenceViewModel.InferenceLoading = Visibility.Visible;
            else
                mediaInferenceViewModel.InferenceLoading = Visibility.Collapsed;
        }

        private async Task DisplayImageAsync(StorageFile storageFile)
        {
            OriginalImage.Source = await ImageHelpers.LoadStorageFileToBitmap(storageFile);
        }

        private async Task PerformDetectionAsync(SixLabors.ImageSharp.Image image)
        {
            var outputImage = await Task.Run(() => predictorService.YoloPredictor.PerformDetectionAsync(image));

            var imageArray = ImageHelpers.ImageToImageArray(outputImage);
            DispatcherQueue.TryEnqueue(async () =>
            {
                InferenceImage.Source = await ImageHelpers.ImageArrayToBitmapImage(imageArray);
                SetLoadingState(false);
            });
        }
    }
}
