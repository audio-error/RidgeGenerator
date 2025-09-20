using RidgeGenerator;
using SkiaSharp;
Ridge test = new Ridge(4);

test.Iterate(128);
int[,] map = test.getPixelMap();
byte[,,] byteMap = ConvertToImage.IntArrayToByteArray(map);

SKBitmap bmap = ConvertToImage.ArrayToImage(byteMap);
using FileStream fs = new("test2.png", FileMode.Create);
bmap.Encode(fs, SKEncodedImageFormat.Png, quality: 100);

Console.WriteLine("Saved to {0}",fs.Name);

//test.Debug();