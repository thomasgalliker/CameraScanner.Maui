using System.Diagnostics;

namespace CameraDemoApp.Views;

public partial class FilePickerPage : ContentPage
{
    public FilePickerPage()
    {
        this.InitializeComponent();
    }

    private void Image_SizeChanged(object? sender, EventArgs e)
    {
        var visualElement = (VisualElement)sender;
        Debug.WriteLine($"Image_SizeChanged: w:{visualElement.Width} x h:{visualElement.Height}");
    }

    private void BarcodeResultOverlay_SizeChanged(object? sender, EventArgs e)
    {
        var visualElement = (VisualElement)sender;
        Debug.WriteLine($"BarcodeResultOverlay_SizeChanged: w:{visualElement.Width} x h:{visualElement.Height}");
    }
}