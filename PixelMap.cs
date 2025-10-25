using System.Text.Unicode;
using SkiaSharp;

namespace RidgeGenerator;

//simple byte map of pixels that has some functions for scaling
public class PixelMap
{
    public int Size; //Number of Pixels in the maps

    public int GridSize => _pixelMap.GetLength(0); //length of the map (Square) TODO: make this a rectangle
    protected byte[,] _pixelMap;
    
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

    protected void Clear()
    {
        _pixelMap = new byte[GridSize, GridSize];
        Size = 0;
    }

    //combine two pixelmaps together
    public void AddPixelMap(PixelMap other)
    {
        for (int y = 0; y < other.GridSize; y++)
        {
            for (int x = 0; x < other.GridSize; x++)
            {
                if (other.GetPixel(x, y) > 0)
                {
                    double value = GetPixel(x,y) + other.GetPixel(x, y);
                    if (value > 200) value = 200; 
                    _pixelMap[x, y] = (byte)value;
                }
            }
        }
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

    //Draw the grid to the console
    public void DrawGrid(bool numbers=false)
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
                if (_pixelMap[j, i] > 0)
                {
                    if (numbers) Console.Write(_pixelMap[j, i]);
                    Console.Write("█"); 
                    pCount++;
                }
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

    public byte[,] GetPixelMap(int x, int y)
    {
        return _pixelMap;
    }
    
}