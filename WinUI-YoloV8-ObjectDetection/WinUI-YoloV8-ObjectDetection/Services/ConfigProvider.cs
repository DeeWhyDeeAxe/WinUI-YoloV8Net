using System;
using System.IO;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Model;

namespace WinUI_YoloV8_ObjectDetection.Services
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly string _cocoDataSetFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/coco-labels-2014-2017.txt");
        public ApplicationConfig _applicationConfig { get; set; } = new ApplicationConfig();

        public ConfigProvider()
        {
            LoadCocoDataset();
        }

        public ApplicationConfig GetConfig()
        {
            if (_applicationConfig == null)
                LoadCocoDataset();
            return _applicationConfig;
        }

        public void LoadCocoDataset()
        {
            if (File.Exists(_cocoDataSetFilePath))
            {
                string cocoDataSetText = File.ReadAllText(_cocoDataSetFilePath);
                _applicationConfig.CocoDataset = cocoDataSetText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
        }
    }
}
