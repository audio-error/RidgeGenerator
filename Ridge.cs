using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;

namespace RidgeGenerator;

public class Ridge
{
    public int targetSize;
    private int _size;
    private int[,] _pixelMap;
    Random _random;

    public struct Coordinate
    {
        public int x; 
        public int y;
    };
    
    //creates a square grid of pixels of length size. Initialises with one random pixel, uses default seed
    public Ridge(int size)
    {
        this.targetSize = size;
        this._pixelMap = new int[4,4];
        this._random = new Random();
        _CreateGrid();
    }

    //creates a square grid of pixels of length size. Initialises with one random pixel, uses specified seed for randomness
    public Ridge(int size, int seed)
    {
        this.targetSize = size;
        this._pixelMap = new int[4,4];
        this._random = new Random(seed);
        _CreateGrid();
    }

    //initialise all the positions in the grid as zero, then make one random 0 to 1
    private void _CreateGrid()
    {
        _pixelMap.Initialize();
        _size = _pixelMap.GetLength(0);
        //make one random position true (add a pixel there)
        _pixelMap[_random.Next(_size - 1), _random.Next(_size - 1)] = 1;
    }

    //expands the grid by x amount from the center outwards evenly. Grids will not go from odd to even.
    private void ExpandGrid(int amount)
    {
        int adjustAmount = amount;
        amount *= 2;
        int originalSize = _pixelMap.GetLength(0);
        int[,] newGrid = new int[originalSize + amount, originalSize + amount];
        newGrid.Initialize(); //set all positions to 0
        for (int x = 0; x < originalSize; x++)
        {
            for (int y = 0; y < originalSize; y++)
            {
                newGrid[x+adjustAmount, y+adjustAmount] = _pixelMap[x, y];
            }
        }
        _pixelMap = newGrid;
        _size = newGrid.GetLength(0);
        //Console.WriteLine("New grid size is {0}", newGrid.GetLength(0));
    }

    //Add a new pixel to the grid, then move it in random directions until it is next to another pixel
    //returns the final position of the pixel
    private Coordinate CreateNewPixel()
    {
        int newPixel = 1; //this will be an object in the future possibly?
        Coordinate newPosition = new Coordinate
        {
            x = _random.Next(_size - 1),
            y = _random.Next(_size - 1)
        };
        //If the position already has a pixel, change starting position until we find an empty position.
        int c = 0;
        while (_pixelMap[newPosition.x, newPosition.y] == 1)
        {
            if (c == 100) { throw(new Exception("This grid is probably full or you are incredibly unlucky."));}
            newPosition.x = _random.Next(_size - 1);
            newPosition.y = _random.Next(_size - 1);
            c++;
        }

        //Keep moving in random directions until we are next to another pixel
        c = 0;
        while (!CheckPosition(newPosition))
        {
            if (c == 10000) { throw new Exception( string.Format("Timed out. Couldn't find a neighbor after {0} tries", c) );}

            newPosition.x = newPosition.x + _random.Next(-1,2);
            newPosition.y = newPosition.y + _random.Next(-1,2);
            
            //old depreciated
            /*newPosition.x = _random.Next(_size - 1);
            newPosition.y = _random.Next(_size - 1);*/

            c++;
        }
        
        
        /*
        c = 0;
        do
        {
            if (c == 10) { break;}
            newPosition.x = _random.Next(_size - 1);
            newPosition.y = _random.Next(_size - 1);
            c++;
        } while (!CheckPosition(newPosition));
        */
        
        _pixelMap[newPosition.x, newPosition.y] = newPixel;
        return newPosition;
    }
    
    //check the 8 positions around the pixel to see if there are any neighbors
    private bool CheckPosition(Coordinate p)
    {
        //these are the coodinates for the surroudning pixels
        int[,] positions =
        {
            /* Diagonal */  {p.x, p.y-1},  /* Diagonal */
            {p.x-1, p.y  }, /* Our Pos */  {p.x+1, p.y  },
            /* Diagonal */  {p.x, p.y+1},  /* Diagonal */
        };
        
        int[,] positionsLarge =
        {
            {p.x-1, p.y-1},  {p.x, p.y-1},  {p.x+1, p.y-1},
            {p.x-1, p.y  }, /* Our Pos */  {p.x+1, p.y},
            {p.x-1, p.y-1},  {p.x, p.y+1},  {p.x+1, p.y-1}
        };

        int currentSize = _pixelMap.GetLength(0);
        for (int i = 0; i < positions.GetLength(0); i++)
        {
            if (positions[i,0] < 0 || positions[i,0] > (currentSize-1) || positions[i,1] < 0 || positions[i,1] > (currentSize-1)){continue;}//skip this iteration if we exit the bounds of the array
            if (_pixelMap[positions[i,0],positions[i,1]] == 1) {return true;}
        }
        
        return false;
    }

    //populate the grid with pixels, this will expand the grid exponentially
    public void Iterate(int numberOfIterations, double density = 1)
    {
        //create the initial pixels
        Console.WriteLine("Creating grid.");

        int fails = 0;
        for (int j = 0; j < _pixelMap.GetLength(0) * density; j++)
        {
            try { CreateNewPixel(); Console.Write(".");}
            catch (Exception e) { Console.WriteLine(e); fails++; Console.Write("x");
            }
        }
        //Console.WriteLine("Added {0} starting pixels, skipped {1} Pixels", _pixelMap.GetLength(0) * 2, fails);
        
        for (int i = 0; i < numberOfIterations; i++)
        {
            int previousFails = 0;
            //now increase by 1
            ExpandGrid(2);
            //for each change in the size of the grid, add pixels equal to twice the size of the grid
            for (int j = 0; j < _pixelMap.GetLength(0) * 2; j++)
            {
                previousFails = fails;
                try
                {
                    CreateNewPixel(); 
                    //Console.Write(".");
                }
                catch (Exception e) 
                { 
                    fails++;
                    //Console.Write("x");
                }
            }

            //Console.WriteLine("Added {0} more pixels, skipped {1} Pixels", _pixelMap.GetLength(0) * 2, fails-previousFails);
        }
        Console.WriteLine("Finished. Total Pixels: {0}, Total Skipped Pixels: {1}", _pixelMap.GetLength(0) * 2, fails );
    }
    
    //Draw the grid to the console as 0s and 1s
    public void DrawGrid()
    {
        Console.WriteLine("Drawing Grid of size: {0}", _pixelMap.GetLength(0));
        for (int i = 0; i < _pixelMap.GetLength(0); i++)
        {
            for (int j = 0; j < _pixelMap.GetLength(1); j++)
            {
                Console.Write("|{0,1}|",_pixelMap[j, i]);
            }

            Console.Write('\n');
        }
    }

    public void Debug()
    {
        _pixelMap.Initialize();
        ExpandGrid(4);
        Console.WriteLine("Starting Debugging, press space to continue after each step.");
        Console.ReadKey();

        for (int i = 0; i < 24; i++)
        {
            _pixelMap[_random.Next(_pixelMap.GetLength(0) - 1), _random.Next(_pixelMap.GetLength(0) - 1)] = 1;
        }

        for (int i = 0; i < _pixelMap.GetLength(0) * 2; i++)
        {
            CreateNewPixel();
        }
        
        DrawGrid(); 
    }

    public int[,] getPixelMap()
    {
        return _pixelMap;
    }

    public void createImage()
    {
        
    }
    
}