using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace SharpDXCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var img = CaptureScreenFrames(0);
            img.Save("bitmap.png", ImageFormat.Png);
            Console.ReadLine();
        }

        public static Bitmap CaptureScreenFrames(int screenId)
        {
            var factory = new Factory1();
            var adapter = factory.Adapters1[0];
            var output = adapter.Outputs[screenId];
            var output1 = output.QueryInterface<Output1>();
            var device = new SharpDX.Direct3D11.Device(adapter);

            var bounds = output1.Description.DesktopBounds;
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
                Usage = ResourceUsage.Staging
            };

            var duplication = output1.DuplicateOutput(device);
            var currentFrame = new Texture2D(device, textureDesc);

            Thread.Sleep(100);

            duplication.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);

            var tempTexture = desktopResource.QueryInterface<Texture2D>();
            device.ImmediateContext.CopyResource(tempTexture, currentFrame);

            var dataBox = device.ImmediateContext.MapSubresource(currentFrame, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

            var frame = new Bitmap(1920, 1080, PixelFormat.Format32bppRgb);
            var mapDest = frame.LockBits(new Rectangle(0, 0, 1920, 1080), ImageLockMode.WriteOnly, frame.PixelFormat);
            for (int y = 0, sizeInBytesToCopy = 1920 * 4; y < 1080; y++)
            {
                Utilities.CopyMemory(mapDest.Scan0 + y * mapDest.Stride, dataBox.DataPointer + y * dataBox.RowPitch, sizeInBytesToCopy);
            }
            frame.UnlockBits(mapDest);

            return frame;
        }
    }
}
