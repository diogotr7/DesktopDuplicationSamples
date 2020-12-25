using SharpGen.Runtime;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

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
            img.Save("bitmap.png", ImageFormat.Png);
            Console.ReadLine();
        }

        public static Bitmap CaptureScreenFrames(int screenId)
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

            var frame = new Bitmap(1920, 1080, PixelFormat.Format32bppRgb);
            var mapDest = frame.LockBits(new Rectangle(0, 0, 1920, 1080), ImageLockMode.WriteOnly, frame.PixelFormat);
            for (int y = 0, sizeInBytesToCopy = 1920 * 4; y < 1080; y++)
            {
                MemoryHelpers.CopyMemory(mapDest.Scan0 + y * mapDest.Stride, dataBox.DataPointer + y * dataBox.RowPitch, sizeInBytesToCopy);
            }
            frame.UnlockBits(mapDest);

            return frame;
        }
    }
}
