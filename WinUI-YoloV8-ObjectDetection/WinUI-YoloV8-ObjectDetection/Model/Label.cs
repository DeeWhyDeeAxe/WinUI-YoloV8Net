namespace WinUI_YoloV8_ObjectDetection.Model
{
    public class Label
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public LabelKind Kind { get; init; } = LabelKind.Generic;
    }

    public enum LabelKind
    {
        Generic,
        InstanceSeg,
    }
}
