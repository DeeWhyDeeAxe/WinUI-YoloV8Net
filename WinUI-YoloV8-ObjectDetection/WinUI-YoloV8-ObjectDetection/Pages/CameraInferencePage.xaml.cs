using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp.PixelFormats;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Utilities;

namespace WinUI_YoloV8_ObjectDetection.Pages
{
    public sealed partial class CameraInferencePage : Page
    {
        private IPredictor yoloV8Predictor;
        private ISettingsService settingsService;
        private IPredictorService predictorService;

        private MediaCapture mediaCapture;
        private MediaSource mediaSource;
        private MediaFrameReader mediaFrameReader;
        public CameraInferencePage()
        {

            this.InitializeComponent();

            settingsService = App.Current.ServiceProvider.GetService<ISettingsService>();
            predictorService = App.Current.ServiceProvider.GetRequiredService<IPredictorService>();
            settingsService.ApplySettings();

            StartCamera();
        }

        private async void StartCamera()
        {
            await StartCameraCaptureAsync(0);
        }

        private async Task StartCameraCaptureAsync(int cameraIdx)
        {
            var mediaFramesourceGroup = await MediaFrameSourceGroup.FindAllAsync();
            try
            {
                var mediaFrameSource = mediaFramesourceGroup.FirstOrDefault();
                mediaCapture = new MediaCapture();

                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                {
                    SourceGroup = mediaFrameSource,
                    SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu
                });

                mediaSource = MediaSource.CreateFromMediaFrameSource(mediaCapture.FrameSources[mediaFrameSource.SourceInfos[cameraIdx].Id]);
                WebcamMediaElement.Source = mediaSource;

                // MediaFrameReader for inference
                mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(mediaCapture.FrameSources[mediaFrameSource.SourceInfos[cameraIdx].Id], MediaEncodingSubtypes.Argb32);
                mediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;
                await mediaFrameReader.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var mediaFrameReference = sender.TryAcquireLatestFrame();
            if (mediaFrameReference != null)
            {
                var inputBitmap = mediaFrameReference.VideoMediaFrame?.SoftwareBitmap;
                if (inputBitmap != null)
                {
                    var inferenceImage = await predictorService.YoloPredictor.PerformDetectionAsync(ImageHelpers.LoadImageSharpImage<Argb32>(inputBitmap));
                    var image = ImageHelpers.ImageToImageArray(inferenceImage);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        WebcamInferenceImage.Source = await ImageHelpers.ImageArrayToBitmapImage(image);
                    });
                }
            }
        }

        private async Task StopCameraCaptureAsync()
        {
            mediaSource?.Dispose();
            mediaSource = null;

            if (mediaFrameReader != null)
            {
                await mediaFrameReader.StopAsync();
                mediaFrameReader.Dispose();
                mediaFrameReader = null;
            }

            mediaCapture.Dispose();
            mediaCapture = null;
        }
    }
}
