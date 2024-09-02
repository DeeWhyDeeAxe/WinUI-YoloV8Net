using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Model;
using WinUI_YoloV8_ObjectDetection.Utilities;

namespace WinUI_YoloV8_ObjectDetection
{
    internal class YoloV8Predictor : YoloPredictorBase, IPredictor
    {
        public static IPredictor Create(string modelPath, string[]? labels = null, bool? useCuda = false, bool useQnn = false)
        {
            return new YoloV8Predictor(modelPath, labels, useCuda, useQnn);
        }

        private YoloV8Predictor(string modelPath, string[]? labels = null, bool? useCuda = false, bool useQnn = false)
            : base(modelPath, labels, useCuda, useQnn) { }

        protected List<Prediction> ParseOutput(DenseTensor<float> output, Image image)
        {
            var result = new ConcurrentBag<Prediction>();

            var (w, h) = (image.Width, image.Height);
            var (xGain, yGain) = (InferenceMetadata.ModelInputWidth / (float)w, InferenceMetadata.ModelInputHeight / (float)h);
            var (xPad, yPad) = ((InferenceMetadata.ModelInputWidth - w * xGain) / 2, (InferenceMetadata.ModelInputHeight - h * yGain) / 2);

            var batchSize = output.Dimensions[0];
            var elementPerPrediction = output.Length / output.Dimensions[1];
            var numPredictions = elementPerPrediction * batchSize;

            //for each batch
            Parallel.For(0, numPredictions, idx =>
            {
                int i = (int)(idx / elementPerPrediction);
                int j = (int)(idx % elementPerPrediction);

                float xMin = ((output[i, 0, j] - output[i, 2, j] / 2) - xPad) / xGain;
                float yMin = ((output[i, 1, j] - output[i, 3, j] / 2) - yPad) / yGain;
                float xMax = ((output[i, 0, j] + output[i, 2, j] / 2) - xPad) / xGain;
                float yMax = ((output[i, 1, j] + output[i, 3, j] / 2) - yPad) / yGain;

                xMin = ImageHelpers.Clamp(xMin, 0, w - 0);
                yMin = ImageHelpers.Clamp(yMin, 0, h - 0);
                xMax = ImageHelpers.Clamp(xMax, 0, w - 1);
                yMax = ImageHelpers.Clamp(yMax, 0, h - 1);

                for (int l = 0; l < InferenceMetadata.ModelOutputDimensions - 4; l++)
                {
                    var pred = output[i, 4 + l, j];

                    if (pred < Confidence) continue;

                    result.Add(new Prediction()
                    {
                        Label = InferenceMetadata.Labels[l],
                        ConfidenceScore = pred,
                        BoundingBox = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                    });
                }
            });
            return result.ToList();
        }

        private async Task<Image> DrawBoxesAsync(int modelInputHeight, int modelInputWidth, Image image, Prediction[] predictions)
        {
            return await Task.Run(() =>
            {
                var originalImageHeight = image.Height;
                var originalImageWidth = image.Width;

                FontFamily fontFamily;
                if (!SystemFonts.TryGet("Arial", out fontFamily))
                    throw new Exception("Invalid font");

                var font = fontFamily.CreateFont(15, FontStyle.Regular);

                foreach (var pred in predictions)
                {
                    var rect = new Rectangle(
                        (int)Math.Max(pred.BoundingBox.X, 0),
                        (int)Math.Max(pred.BoundingBox.Y, 0),
                        (int)Math.Min(originalImageWidth - pred.BoundingBox.X, pred.BoundingBox.Width),
                        (int)Math.Min(originalImageHeight - pred.BoundingBox.Y, pred.BoundingBox.Height)
                    );

                    var text = $"{pred.Label.Name} [{pred.ConfidenceScore}]";
                    var size = TextMeasurer.MeasureSize(text, new TextOptions(font));

                    image.Mutate(d => d
                        .Draw(Pens.Solid(Color.Red, 2), rect)
                        .Fill(Color.Red, new Rectangle(rect.X, rect.Y - (int)size.Height, (int)size.Width, (int)size.Height))
                        .DrawText(text, font, Color.White, new Point(rect.X, rect.Y - (int)size.Height - 1)));
                }
                return image;
            }).ConfigureAwait(false);
        }

        public override async Task<Image> PerformDetectionAsync(Image image)
        {
            var predictions = SuppressPredictions(
                ParseOutput(Inference(image)[0], image));

            return await DrawBoxesAsync(1000, 1000, image, predictions);
        }
    }
}
