namespace WinUI_YoloV8_ObjectDetection.Model
{
    public class InferenceMetadata
    {
        public InferenceMetadata() { }

        // Input metadatas
        private string inputColumnName;
        private int modelInputHeight;
        private int modelInputWidth;
        // Output metadatas
        private string outputColumnName;
        private string[] modelOutputs;
        private int modelOutputDimensions;

        private Label[] labels;

        public string InputColumnName { get { return inputColumnName; } set { inputColumnName = value; } }
        public int ModelInputHeight { get { return modelInputHeight; } set { modelInputHeight = value; } }
        public int ModelInputWidth { get { return modelInputWidth; } set { modelInputWidth = value; } }

        public string OutputColumnName { get { return outputColumnName; } set { outputColumnName = value; } }
        public string[] ModelOutputs { get { return modelOutputs; } set { modelOutputs = value; } }
        public int ModelOutputDimensions { get { return modelOutputDimensions; } set { modelOutputDimensions = value; } }

        public Label[] Labels { get { return labels; } set { labels = value; } }
    }
}
