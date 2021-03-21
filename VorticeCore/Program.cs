using SharpGen.Runtime;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using SkiaSharp;

namespace VorticeCore
{
    class Program
    {
        private static readonly FeatureLevel[] s_featureLevels = new[]
        {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        };

        static void Main(string[] args)
        {
            var img = CaptureScreenFrames(0);
            using (var data = img.Encode(SKEncodedImageFormat.Png, 80))
            using (var stream = System.IO.File.OpenWrite("skbitmap.png"))
            {
                // save the data to a stream
                data.SaveTo(stream);
            }
        }

        public static SKBitmap CaptureScreenFrames(int screenId)
        {
            var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            var adapter = factory.GetAdapter(0);
            var output = adapter.GetOutput(screenId);
            var output1 = output.QueryInterface<IDXGIOutput1>();
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.None, s_featureLevels, out var device);

            var bounds = output1.Description.DesktopCoordinates;
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Vortice.Direct3D11.Usage.Staging
            };

            var duplication = output1.DuplicateOutput(device);
            var currentFrame = device.CreateTexture2D(textureDesc);

            Thread.Sleep(100);

            duplication.AcquireNextFrame(500, out var frameInfo, out var desktopResource);

            var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
            device.ImmediateContext.CopyResource(currentFrame, tempTexture);

            var dataBox = device.ImmediateContext.Map(currentFrame, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            var skInfo = new SKImageInfo
            {
                ColorType = SKColorType.Bgra8888,
                AlphaType = SKAlphaType.Premul,
                Height = 1080,
                Width = 1920
            };
            var skPixmap = new SKPixmap(skInfo, dataBox.DataPointer);
            var skBitmap = new SKBitmap();
            skBitmap.InstallPixels(skPixmap);
            return skBitmap;
        }
    }
}
