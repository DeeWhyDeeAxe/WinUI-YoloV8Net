# WinUI-YoloV8Net
C# .NET 8 implementation of YoloV8 for real-time object detection on WinUI

# Features
- Media inference (only image is supported for now)
- 
## Getting Started
# Prerequisites
- YoloV8 model exported to ONNX, follow this [guide](https://docs.ultralytics.com/integrations/onnx/) 
- Visual Studio 2022 
- .NET 8 SDK: Make sure you have the .NET 8 SDK installed. You can download it from the [official .NET website](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- WinUI 3 Project Templates: Available through Visual Studio or via the .NET CLI.
- CUDA Toolkit: (Optional) If you want to leverage GPU acceleration for YoloV8, ensure that CUDA and cuDNN is installed and configured. Refer here for the [compatability matrix](https://docs.nvidia.com/deeplearning/cudnn/latest/reference/support-matrix.html)

## Using custom YoloV8 dataset
You may also use your own custom trained model and dataset by placing the label classes within the Resources folder

## Usage
```
// Create a predictor
IPredictor yoloV8Predictor; = YoloV8Predictor.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, <Path to YoloV8 ONNX File>));
// Perform inference on input image
var outputImage = await Task.Run(() => yoloV8Predictor.PredictAsync(image));
 ```

 # Built With
 **.NET 8 
 **WinUI

![](Demo.gif)

Acknowledgments
- Ultralytics for the YoloV8 model
- ONNX Runtime for model inference