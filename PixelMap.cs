using System.Text.Unicode;
using SkiaSharp;

namespace RidgeGenerator;

//simple byte map of pixels that has some functions for scaling and blurring
public class PixelMap
{
    public int Size; //Number of Pixels in the maps

    public int GridSize => _pixelMap.GetLength(0); //length of the map (Square) TODO: make this a rectangle
    private byte[,] _pixelMap;
    
    public PixelMap()
    {
        _pixelMap = new byte[4,4];
        _pixelMap.Initialize();
    }
    

    public bool AddPixel(int x, int y, byte value)
    {
        if (x < 0 || y < 0 || x >= GridSize || y >= GridSize) throw new ArgumentOutOfRangeException();
        if (_pixelMap[x, y] == 0)
        {
            _pixelMap[x, y] = value;
            Size++;
            return true;
        }
        return false;
    }

    public bool UpdatePixel(int x, int y, byte value)
    {
        if (x < 0 || y < 0 || x >= GridSize || y >= GridSize) throw new ArgumentOutOfRangeException();
        if (_pixelMap[x, y] != 0)
        {
            _pixelMap[x, y] = value; 
            return true;
        }    
        return false;
    }

    public byte GetPixel(int x, int y)
    {
        if (x < 0 || y < 0 || x >= GridSize || y >= GridSize) throw new ArgumentOutOfRangeException();
        return _pixelMap[x,y];
    }

    public void Initialize()
    {
        _pixelMap = new byte[GridSize, GridSize];
        Size = 0;
    }
    
    //Doubles the size of the grid and centers the pixels
    public void ExpandGrid()
    {
        int adjustAmount = GridSize * 2;
        int originalSize = GridSize;
        byte[,] newGrid = new byte[adjustAmount, adjustAmount];
        newGrid.Initialize(); //set all positions to 0
        for (int y = 0; y < originalSize; y++)
        {
            for (int x = 0; x < originalSize; x++)
            {
                newGrid[y + originalSize, x + originalSize] = _pixelMap[y,x];
            }
        }
        _pixelMap = newGrid;
        //Console.WriteLine("New grid size is {0}", newGrid.GetLength(0));
    }

    //produces the convulution of the Pixelmap position using the provided 3x3 kernal
    private byte Convolve(double[,] kernal, int x, int y)
    {
        //int currentSize = _pixelMap.GetLength(0); Use _gridSize instead.
        
        List<double> kernalValues = new List<double>();
        for (int i = 0; i < kernal.GetLength(0); i++)
        {
            //Remember that [,] loops are Y,X not X,Y
            //i = y and j = x
            //shift the X and Y values so that the "middle" value of the kernal is 0,0
            int currentY = y + (i - 2);
            for (int j = 0; j < kernal.GetLength(1); j++)
            {
                int currentX = x + (j - 2);
                //Console.WriteLine($"Assessing Value: {currentX}, {currentY}");
                //check the value the kernal is trying to evaluate is inside the image... else ignore this value
                if (currentX < 0 || currentX > (GridSize-1) || currentY < 0 ||
                    currentY > (GridSize-1) || kernal[i,j] == 1 ) continue;

                kernalValues.Add(_pixelMap[currentX, currentY]);
            }
        }


        double result = kernalValues.Average();
        //Console.WriteLine($"Convolution on {x},{y} = {result}");
        if (result > 255) return 255;
        return (byte)result;
    }
    
    //will produce a convoluted blurry version of the image. 
    public byte[,] Convolute()
    {
        byte[,] convolutedImage = new byte[GridSize, GridSize];
        double[,] kernal = new double[5, 5]
        {  //circular kernal for a softer approach
            {  1,     1,   1f/13f,   1,     1   },
            {  1,   1f/13f, 1f/13f, 1f/13f,   1   },
            {1f/13f, 1f/13f, 1f/13f, 1f/13f, 1f/13f },
            {  1,   1f/13f, 1f/13f, 1f/13f,   1   },
            {  1,     1,   1f/13f,   1,     1   }
        };
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                convolutedImage[x,y] = Convolve(kernal,x,y);
            }
        }
        return convolutedImage;
    }

    //Draw the grid to the console
    public void DrawGrid()
    {
        Console.WriteLine("Drawing Grid of size: {0}", _pixelMap.GetLength(0));
        int pCount = 0;
        //draw top row
        Console.Write('┌'); for (int i = 0; i < _pixelMap.GetLength(0); i++) { Console.Write('─');} Console.Write('┐');
        Console.WriteLine();
        for (int i = 0; i < _pixelMap.GetLength(0); i++)
        {
            Console.Write('│');
            for (int j = 0; j < _pixelMap.GetLength(1); j++)
            {
                if (_pixelMap[j, i] > 0) {Console.Write("█"); pCount++;}
                else {Console.Write(" ");}
            }
            Console.Write('│');
            //Console.Write('├'); for (int c = 0; c < _pixelMap.GetLength(0); c++) { Console.Write('─');} Console.Write('┤');
            
            Console.Write('\n');
        }
        Console.Write('└'); for (int i = 0; i < _pixelMap.GetLength(0); i++) { Console.Write('─');} Console.Write('┘');
        //Console.WriteLine($"Pixels placed: {pCount}");
    }

    public byte[,] GetPixelMap()
    {
        return _pixelMap;
    }
    public void SetPixelMap(byte[,] a)
    {
        _pixelMap = a;
    }

    //upscale the grid by linear interpolation
    //Depreciated
    public void UpscaleLinearInterpolation(int amount)
    {
        int adjustAmount = amount;
        
        //covert pixelmap into a bitmap
        SKBitmap newBitmap = ConvertToImage.ByteArrayToImage(ConvertToImage.IntArrayToByteArray(_pixelMap));
        SKBitmap upscaledBitmap = new SKBitmap();
        //then upscale using linear interpolation from Skiasharp
        SKSamplingOptions samplingOptions = new SKSamplingOptions(filter:SKFilterMode.Nearest);
        SKSizeI imageSize = new SKSizeI( _pixelMap.GetLength(0) * adjustAmount, _pixelMap.GetLength(0) * adjustAmount);
        upscaledBitmap = newBitmap.Resize(imageSize, samplingOptions);
        //we're doing it this was because I'm lazy - Sean
        
        _pixelMap = ConvertToImage.BitmapToByteArray(upscaledBitmap);
    }
    
}