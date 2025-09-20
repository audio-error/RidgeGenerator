using RidgeGenerator;
using SkiaSharp;
Ridge test = new Ridge(4);

test.Iterate(64, 2);
int[,] map = test.getPixelMap();
byte[,,] byteMap = ConvertToImage.IntArrayToByteArray(map);

SKBitmap bmap = ConvertToImage.ArrayToImage(byteMap);
using FileStream fs = new("Ridges_Lines.png", FileMode.Create);
bmap.Encode(fs, SKEncodedImageFormat.Png, quality: 100);

Console.WriteLine("Saved to {0}",fs.Name);

//test.Debug();