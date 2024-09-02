using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WinUI_YoloV8_ObjectDetection.Utilities
{
    public static class ImageHelpers
    {
        public static float[] Xywh2xyxy(float[] source)
        {
            var result = new float[4];

            result[0] = source[0] - source[2] / 2f;
            result[1] = source[1] - source[3] / 2f;
            result[2] = source[0] + source[2] / 2f;
            result[3] = source[1] + source[3] / 2f;

            return result;
        }

        public static Image ResizeImage(Image image, int target_width, int target_height)
        {
            return image.Clone(x => x.Resize(target_width, target_height));
        }

        public static Tensor<float> ExtractPixels(Image image)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, image.Height, image.Width });

            using (var img = image.CloneAs<Rgb24>())
            {
                Parallel.For(0, img.Height, y =>
                {
                    var pixelSpan = img.DangerousGetPixelRowMemory((int)y).Span;
                    for (int x = 0; x < img.Width; x++)
                    {
                        tensor[0, 0, y, x] = pixelSpan[x].R / 255.0F; // r
                        tensor[0, 1, y, x] = pixelSpan[x].G / 255.0F; // g
                        tensor[0, 2, y, x] = pixelSpan[x].B / 255.0F; // b
                    }
                });
            }
            return tensor;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Sigmoid(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }

        /// <summary>
        /// Image conversion from ImageSharp Image to byte array
        /// </summary>
        /// <param name="image">Input ImageSharp Image</param>
        /// <returns>Image in byte array form</returns>
        public static byte[] ImageToImageArray(this Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, JpegFormat.Instance);
                return ms.ToArray();
            }
        }

        public static async Task<BitmapImage> ImageArrayToBitmapImage(byte[] byteArray)
        {
            var bitmapImage = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream))
                {
                    writer.WriteBytes(byteArray);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();
                }
                stream.Seek(0);
                bitmapImage.SetSource(stream);
            }
            return bitmapImage;
        }

        public static async Task<BitmapImage> LoadStorageFileToBitmap(StorageFile storageFile)
        {
            BitmapImage image = new BitmapImage();
            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                await image.SetSourceAsync(stream);
            }
            return image;
        }

        public static byte[] SoftwareBitmapToByteArray(SoftwareBitmap softwareBitmap)
        {
            byte[] byteArray = null;

            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream).AsTask().Result;
                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.FlushAsync().AsTask().Wait();

                stream.Seek(0);
                byteArray = new byte[stream.Size];
                stream.ReadAsync(byteArray.AsBuffer(), (uint)stream.Size, InputStreamOptions.None).AsTask().Wait();
            }
            return byteArray;
        }

        public static Image<TPixel> LoadImageSharpImage<TPixel>(SoftwareBitmap softwareBitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            var byteArray = SoftwareBitmapToByteArray(softwareBitmap);

            using (var ms = new MemoryStream(byteArray))
            {
                var image = SixLabors.ImageSharp.Image.Load<TPixel>(ms);
                return image;
            }
        }
    }
}
