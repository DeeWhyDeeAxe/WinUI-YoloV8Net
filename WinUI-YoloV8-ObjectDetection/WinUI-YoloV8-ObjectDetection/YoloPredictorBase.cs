using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Model;
using WinUI_YoloV8_ObjectDetection.Services;
using WinUI_YoloV8_ObjectDetection.Utilities;

namespace WinUI_YoloV8_ObjectDetection
{
    abstract public class YoloPredictorBase : IPredictor
    {
        protected readonly InferenceSession inferenceSession;

        public InferenceMetadata InferenceMetadata { get; init; } = new InferenceMetadata();
        private IConfigProvider configProvider = new ConfigProvider();

        public float Confidence { get; protected set; } = 0.20f;
        public float MulConfidence { get; protected set; } = 0.25f;
        public float Overlap { get; protected set; } = 0.45f;
        public int ModelOutputDimensions { get; protected set; }
        public bool UseDetect { get; set; }

        protected YoloPredictorBase(string modelPath, string[] labels = null, bool? useCuda = false, bool useQNN = false)
        {
            if (useQNN)
            {
                // Qnn options Documented in https://onnxruntime.ai/docs/execution-providers/QNN-ExecutionProvider.html in order to inference
                // from a onnxruntime quantized Yolo model
                Dictionary<string, string> qnnOptions = new Dictionary<string, string>() { { "backend_path", "QnnHtp.dll" }, { "enable_htp_fp16_precision", "1" } };
                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.AppendExecutionProvider("QNN", qnnOptions);
                inferenceSession = new InferenceSession(modelPath, sessionOptions);
            }
            else if (useCuda == true)
            {
                inferenceSession = new InferenceSession(modelPath, SessionOptions.MakeSessionOptionWithCudaProvider());
            }
            else
            {
                inferenceSession = new InferenceSession(modelPath);
            }

            InferenceMetadata.InputColumnName = inferenceSession.InputMetadata.Keys.First();
            InferenceMetadata.ModelInputHeight = inferenceSession.InputMetadata[InferenceMetadata.InputColumnName].Dimensions[2];
            InferenceMetadata.ModelInputWidth = inferenceSession.InputMetadata[InferenceMetadata.InputColumnName].Dimensions[3];

            InferenceMetadata.OutputColumnName = inferenceSession.OutputMetadata.Keys.First();
            InferenceMetadata.ModelOutputs = inferenceSession.OutputMetadata.Keys.ToArray();
            InferenceMetadata.ModelOutputDimensions = inferenceSession.OutputMetadata[InferenceMetadata.ModelOutputs[0]].Dimensions[1];
            UseDetect = !(InferenceMetadata.ModelOutputs.Any(x => x == "score"));

            if (labels != null)
            {
                UseCustomLabels(labels);
            }
            else UseCoCoLabels();

            if (InferenceMetadata.Labels?.Length + 4 > InferenceMetadata.ModelOutputDimensions)
                throw new ArgumentOutOfRangeException("Exceeded number of labels.");
        }

        protected Prediction[] SuppressPredictions(List<Prediction> predictions)
        {
            var Result = new List<Prediction>(predictions);
            foreach (var prediction in predictions)
            {
                foreach (var currentResult in Result.ToList())
                {
                    if (currentResult == prediction) continue;

                    var (rect1, rect2) = (currentResult.BoundingBox, prediction.BoundingBox);

                    var intersectedBoundingBoxes = RectangleF.Intersect(rect1, rect2);

                    float intArea = intersectedBoundingBoxes.Width * intersectedBoundingBoxes.Height;
                    float unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - intArea;
                    float overlap = intArea / unionArea;
                    if (overlap >= 0.45f)
                    {
                        if (prediction.ConfidenceScore >= currentResult.ConfidenceScore)
                        {
                            Result.Remove(currentResult);
                        }
                    }
                }
            }
            return Result.ToArray();
        }
        public void Dispose()
        {
            inferenceSession.Dispose();
        }

        protected void UseCustomLabels(string[] labels)
        {
            InferenceMetadata.Labels = labels.Select((s, i) => new { i, s }).ToList()
                 .Select(x => new Model.Label()
                 {
                     Id = x.i,
                     Name = x.s
                 }).ToArray();
        }

        protected void UseCoCoLabels()
        {
            UseCustomLabels(configProvider.GetConfig().CocoDataset);
        }

        protected virtual DenseTensor<float>[] Inference(Image img)
        {
            Image resized = null;

            if (img.Width != InferenceMetadata.ModelInputWidth || img.Height != InferenceMetadata.ModelInputHeight)
            {
                resized = ImageHelpers.ResizeImage(img, InferenceMetadata.ModelInputWidth, InferenceMetadata.ModelInputHeight); // fit image size to specified input size
            }
            else
            {
                resized = img;
            }

            var inputs = new List<NamedOnnxValue> // add image as onnx input
            {
                NamedOnnxValue.CreateFromTensor(InferenceMetadata.InputColumnName, ImageHelpers.ExtractPixels(resized))
            };

            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result = inferenceSession.Run(inputs); // run inference

            var output = new List<DenseTensor<float>>();

            foreach (var item in InferenceMetadata.ModelOutputs) // add outputs for processing
            {
                output.Add(result.First(x => x.Name == item).Value as DenseTensor<float>);
            };

            return output.ToArray();
        }
        public virtual Task<Image> PerformDetectionAsync(Image img)
        {
            return null;
        }
    }
}
