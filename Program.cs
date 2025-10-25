using System.Drawing;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using RidgeGenerator;
using SkiaSharp;
using Point = RidgeGenerator.Point;

Ridge heightMap = new Ridge(4,10);
//heightMap.Iterate(4);

PrintToImage(heightMap.GetSharpMap(), "Sharp Image.jpg");
PrintToImage(heightMap.GetFuzzyMap(), "Fuzzy Image.jpg");

void PrintToImage(byte[,] map, string name)
{
    byte[,,] byteMap = ConvertToImage.IntArrayToByteArray(map);
    SKBitmap bmap = ConvertToImage.ByteArrayToImage(byteMap);
    SKFileWStream fs = new(name);
    bmap.Encode(fs, SKEncodedImageFormat.Jpeg, quality: 100);
    Console.WriteLine($"Output: {name} to {Path.GetFullPath(name)}");
}
