using System.Runtime.InteropServices;
using SkiaSharp;

namespace RidgeGenerator;

public class ConvertToImage
{
    public static SKBitmap ArrayToImage(byte[,,] pixelArray)
    {
        int width = pixelArray.GetLength(1);
        int height = pixelArray.GetLength(0);

        uint[] pixelValues = new uint[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte alpha = 255;
                byte red = pixelArray[y, x, 0];
                byte green = pixelArray[y, x, 1];
                byte blue = pixelArray[y, x, 2];
                uint pixelValue = (uint)red + (uint)(green << 8) + (uint)(blue << 16) + (uint)(alpha << 24);
                pixelValues[y * width + x] = pixelValue;
            }
        }

        SKBitmap bitmap = new();
        GCHandle gcHandle = GCHandle.Alloc(pixelValues, GCHandleType.Pinned);
        SKImageInfo info = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        IntPtr ptr = gcHandle.AddrOfPinnedObject();
        int rowBytes = info.RowBytes;
        bitmap.InstallPixels(info, ptr, rowBytes, delegate { gcHandle.Free(); });

        return bitmap;
    }
    
    public static byte[,,] IntArrayToByteArray(int[,] pixelArray)
    {
        int width = pixelArray.GetLength(0);
        int height = pixelArray.GetLength(0);

        byte[,,] byteArray = new byte[width, height, 4];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                byte alpha = Convert.ToByte(pixelArray[x,y] * 255);
                byte red = Convert.ToByte(pixelArray[x,y] * 255);
                byte green = Convert.ToByte(pixelArray[x,y] * 255);
                byte blue = Convert.ToByte(pixelArray[x,y] * 255);
                
                byteArray[y, x, 0] = alpha;
                byteArray[y, x, 1] = red;
                byteArray[y, x, 2] = green;
                byteArray[y, x, 3] = blue;
            }
        }
        
        return byteArray;
    }
}