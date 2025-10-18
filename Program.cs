using System.Drawing;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using RidgeGenerator;
using SkiaSharp;


Ridge heightMap = new Ridge(4,2);

//heightMap.CreateSinglePixel(220);





printToImage(heightMap.GetSharpMap(), "Sharp Image.jpg");
printToImage(heightMap.GetFuzzyMap(), "Fuzzy Image.jpg");

void printToImage(byte[,] map, string name)
{
    byte[,,] byteMap = ConvertToImage.IntArrayToByteArray(map);
    SKBitmap bmap = ConvertToImage.ByteArrayToImage(byteMap);
    SKFileWStream fs = new(name);
    bmap.Encode(fs, SKEncodedImageFormat.Jpeg, quality: 100);
}



/*
map = heightMap.GetPixelMap();
byteMap = ConvertToImage.IntArrayToByteArray(map);
bmap = ConvertToImage.ByteArrayToImage(byteMap);
fs = new("Fuzzy Image.jpg");
bmap.Encode(fs, SKEncodedImageFormat.Jpeg, quality: 100);
*/
