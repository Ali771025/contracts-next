namespace Contracts.Desktop;

using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class VisualSmokeCapture
{
    private static readonly int[] ScalePercentages = [100, 125, 150];

    public static IReadOnlyList<string> Capture(Window window, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!Path.IsPathFullyQualified(outputDirectory))
            throw new ArgumentException("Visual smoke output must be an absolute path.", nameof(outputDirectory));
        Directory.CreateDirectory(outputDirectory);
        List<string> files = [];
        foreach (int percentage in ScalePercentages)
        {
            double scale = percentage / 100d;
            double dpi = 96d * scale;
            int pixelWidth = checked((int)Math.Ceiling(window.ActualWidth * scale));
            int pixelHeight = checked((int)Math.Ceiling(window.ActualHeight * scale));
            DrawingVisual visual = new();
            using (DrawingContext drawing = visual.RenderOpen())
            {
                drawing.DrawRectangle(new VisualBrush(window), null,
                    new Rect(0, 0, window.ActualWidth, window.ActualHeight));
            }
            RenderTargetBitmap bitmap = new(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            string path = Path.Combine(outputDirectory, $"shell-rtl-{percentage}pct.png");
            using FileStream stream = File.Create(path);
            encoder.Save(stream);
            files.Add(path);
        }
        return files;
    }
}
