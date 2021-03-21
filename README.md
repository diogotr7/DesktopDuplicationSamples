# DesktopDuplicationSamples

Sample project i wrote when i was having difficulties porting some SharpDX desktop duplication code to Vortice.

The issue ended up being the different order of arguments in [this](https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-copyresource) function.
