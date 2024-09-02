using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI_YoloV8_ObjectDetection.Pages;

namespace WinUI_YoloV8_ObjectDetection
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        private readonly Type DefaultFrame = typeof(MediaInferencePage);
        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            ExtendsContentIntoTitleBar = true;

            PageViewFrame.Navigate(DefaultFrame);
            NavView.SelectionChanged += NavView_SelectionChanged;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;

            if (selectedItem == null)
                return;

            if (args.IsSettingsSelected)
            {
                PageViewFrame.Navigate(typeof(Settings));
            }
            else
            {
                PageViewFrame.Navigate(Type.GetType(selectedItem.Tag.ToString()));
            }
        }
    }
}
