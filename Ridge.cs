using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;

namespace RidgeGenerator;

//This is a simple DLA Algorithm that combines two upscaling techniques to make the image look like mountains
public class Ridge
{
    public int targetSize;
    //private PixelGrid _pixelGrid; //grid object, stores pixel data
    private readonly Random _random;
    private PixelConnectionMap _connectionMap;
    private PixelMap _sharpMap = new PixelMap(); //the pixel map we apply brownian fractals to.
    private PixelMap _fuzzyMap = new PixelMap(); //the pixel map we blur and add our sharp map too to create` a height map
    
    //creates a square grid of pixels of length size. Initialises with one random pixel, uses default seed
    //creates an object to store which pixels get stuck to what
    public Ridge(int size)
    {
        this.targetSize = size;
        this._random = new Random();
        this._connectionMap = new PixelConnectionMap(size);
        _CreateGrid();
    }

    //creates a square grid of pixels of length size. Initialises with one random pixel, uses specified seed for randomness
    public Ridge(int size, int seed)
    {
        this.targetSize = size;
        this._random = new Random(seed);
        this._connectionMap = new PixelConnectionMap(size);
        _CreateGrid();
    }

    //Make one random position max height
    private void _CreateGrid(byte startHeight = 255)
    {
        int posX = _random.Next(_sharpMap.GridSize - 1); 
        int posY = _random.Next(_sharpMap.GridSize - 1);
        _sharpMap.AddPixel(posX, posY, startHeight);
        _connectionMap.AddPoint(new Point(posX, posY, startHeight));
        
        Populate(2,height:255);
        
        _fuzzyMap.SetPixelMap(_sharpMap.GetPixelMap());
    }

    //this will move the pixel in a random direction and ensure it doesn't leave the bounds of the grid
    private Coordinate MoveInRandomDirection(Coordinate pos, out Coordinate direction)
    {
        int newX = 0, newY = 0;
        int maxX = _sharpMap.GridSize-1;
        int maxY = _sharpMap.GridSize-1;
        if (pos.x <= 0)
        {
            newX = _random.Next(0, 2);
        }
        else if (pos.x >= maxX)
        {
            newX = _random.Next(-1, 1);
        }
        else
        {
            newX = _random.Next(-1, 2);
        }
            
        if (pos.y <= 0)
        {
            newY = _random.Next(0, 2);
        }
        else if (pos.y >= maxY)
        {
            newY = _random.Next(-1, 1);
        }
        else
        {
            newY = _random.Next(-1, 2);
        }
        
        direction = new Coordinate(newX, newY);
        return pos + direction;
    }
    
    // Add a new pixel to the grid, then move it in random directions until it is next to another pixel
    //returns the final position of the pixel
    private Coordinate CreateNewPixel(byte height = 255, int tries = 1000)
    {
        byte newPixel = height; //this will be an object in the future possibly?
        //spawn the pixel in a random position
        Coordinate newPosition = new Coordinate
        (
            _random.Next(_sharpMap.GridSize),
            _random.Next(_sharpMap.GridSize),
            newPixel
        );
        //Keep trying to add a new pixel
        int c = 0;
        while (_sharpMap.GetPixel(newPosition.x, newPosition.y) > 0)
        {
            if (c == tries) { throw(new Exception("This grid is probably full or you are incredibly unlucky."));}
            newPosition.x = _random.Next(_sharpMap.GridSize);
            newPosition.y = _random.Next(_sharpMap.GridSize);
            c++;
        }
        
        Coordinate direction = new Coordinate(0,0);//direction we just moved

        //Keep moving in random directions until we are next to another pixel, caps at c number of moves
        c = 0;
        while (!CheckPosition(newPosition, direction))
        {
            if (c == tries) { throw new Exception($"Timed out. Couldn't find a neighbor after {c} tries. Pixel's final coords are {newPosition.x}, {newPosition.y}"); }
            
            newPosition = MoveInRandomDirection(newPosition, out direction);
            c++;
        }
        _sharpMap.AddPixel(newPosition.x, newPosition.y, newPixel);
        return newPosition;
    }

    private void StickTo(Coordinate pos, Coordinate neighbor)
    {
        _connectionMap.AddPoint(pos.x, pos.y, neighbor.x, neighbor.y, pos.weight);
        //Console.WriteLine($"{pos} has stuck to {neighbor} and has a weight of {_sharpMap[pos.x, pos.y]}");
    }
    
    //check the (8|4) positions around the pixel to see if there are any neighbors
    //then connect to the neighbor it ran in to
    private bool CheckPosition(Coordinate p, Coordinate direction)
    {
        Coordinate neighbor;
        int currentSize = _sharpMap.GridSize;
        
        //these are the coodinates for the surroudning pixels
        int[,] positions =
        {
            /* Diagonal */  {p.x, p.y-1},  /* Diagonal */
            {p.x-1, p.y  }, /* Our Pos */  {p.x+1, p.y  },
            /* Diagonal */  {p.x, p.y+1},  /* Diagonal */
        };
        
        //includes the diagonals in the check
        /*int[,] positionsLarge =
        {
            {p.x-1, p.y-1},  {p.x, p.y-1},  {p.x+1, p.y-1},
            {p.x-1, p.y  },  /* Our Pos #1#  {p.x+1, p.y},
            {p.x-1, p.y-1},  {p.x, p.y+1},  {p.x+1, p.y-1}
        };*/
        
        //debug
        //DEBUG: Console.Write("Checking Positions: ");
        List<Coordinate> neighbors = new List<Coordinate>();
        for (int i = 0; i < positions.GetLength(0); i++)//for each position around us
        {
            Coordinate neighboringPixel = new Coordinate(positions[i, 0], positions[i,1]);
            if (positions[i,0] < 0 || positions[i,0] > (currentSize-1) || positions[i,1] < 0 || positions[i,1] > (currentSize-1)){continue;}//skip this iteration if we exit the bounds of the array

            
            if (_sharpMap.GetPixel(neighboringPixel.x, neighboringPixel.y) > 0) //check the value of the neighboring pixel
            {
                //Console.WriteLine($"{neighboringPixel} and {neighboringPixel + direction} are equal");
                if (neighboringPixel == p - direction) //connect to the neighbor in front of us
                {
                    neighbor = neighboringPixel;
                    StickTo(p, neighbor);
                    //Console.WriteLine($"We found a neighbor! Us: {p} is now connected to {neighbor}");
                    return true;
                }

                //if we have a neighbor, but it is not in front of us (i.e. we spawned next to them, or we moved to the side of them)
                neighbors.Add(neighboringPixel);
                //add that neighbor to a list (it is possible to have more than one neighbor)
            }
        }
        //DEBUG: Console.WriteLine($"\nWe have {neighbors.Count} Neighboring Points\n");
        if (neighbors.Count > 0)                 
        {
            neighbor = neighbors[0];
            //Console.WriteLine($"We found {neighbors.Count} neighbors! Us: {p} is now connected to {neighbors[0]}");
            StickTo(p, neighbor);
            return true;
        }
        //we have no neighbors, move again until we find one.
        neighbor = p;//Returns the neighbor as ourselves
        return false;
        //IGNORE THIS -> OLD INFO. since the coordinates never enter the negative plane we can safely use this as a null value
    }

    //populate the sharp grid with pixels equal to density times the volume
    public void Populate(double density = 1.00, byte height = 255, int fails = 0)
    {
        int numOfPixelsToPlace = (int)(_sharpMap.GridSize * _sharpMap.GridSize /16 * density);
        //for each change in the size of the grid, add pixels equal to twice the size of the grid
        for (int j = 0; j < numOfPixelsToPlace * density; j++)
        {
            try { CreateNewPixel(height, tries:100000); Console.Write(".");}
            catch (Exception e) { fails++; Console.Write('X');
            }
        }
        Console.Write('\n');
    }

    public void CreateSinglePixel(byte height = 255)
    {
        CreateNewPixel(height);

        for (int y = 0; y < _sharpMap.GridSize; y++)
        {
            Console.Write('\n');
            for (int x = 0; x < _sharpMap.GridSize; x++)
            {
                Console.Write(_sharpMap.GetPixel(x, y));
                Console.Write(", ");
            }
        }
    }
    
    public byte[,] Combine(PixelMap a, PixelMap b)
    {
        byte[,] newPixelMap = new byte[b.GridSize, b.GridSize];
        for (int y = 0; y < newPixelMap.GetLength(0); y++)
        {
            for (int x = 0; x < newPixelMap.GetLength(1); x++)
            {
                int newHeight = a.GetPixel(x, y) + b.GetPixel(x, y);
                newHeight = 1/(1+newHeight)*255;
                newPixelMap[y, x] = (byte)newHeight;
            }
        }

        return newPixelMap;
    } 
    
    private void MapConnectionMap()
    {
        //wipe the original points
        _sharpMap.Initialize();
        var points = _connectionMap.GetPoints();
        int c = 0;
        foreach (Point pixel in points)
        {
            _sharpMap.AddPixel(pixel.x, pixel.y, (byte)pixel.Weight);
            //Console.WriteLine($"Pixel {pixel}-{pixel.Weight}");
            c++;
        }
        Console.WriteLine($"The size of our ConnectionMap is {_connectionMap.Size}, and we placed {c} Pixels");
    }
    
    //populate the grid with pixels, this will expand the grid exponentially
    //depreciated
    //...
    
    //double this size of the map by using the record of which pixels got stuck to which.
    public void UpscaleSharp()
    {
        //First Expand the grid to twice the size.
        _sharpMap.ExpandGrid();
        //then make sure all the pixels in the connection grid are updated too.
        _connectionMap.UpscaleDouble(1);
        //finally map the connections onto the pixel grid
        MapConnectionMap();
    }
    
    //double the size of the map by using Convolution
    public void UpscaleFuzzy()
    {
        //upscale the map to twice it's size
        //linearly interpolate the values on the bigger grid
        _fuzzyMap.UpscaleLinearInterpolation(_fuzzyMap.GridSize);
        //perform a convolution on this image to blur it
        _fuzzyMap.Convolute();
    }

    //this will run an iteration of the algorithm.
    //upscales crisp and upscales fuzzy, then combines them to produce a heightmap
    public void Itterate()
    { 
        Populate(height:240);
        UpscaleSharp();
        
    }

    public byte[,] GetSharpMap()
    {
        return _sharpMap.GetPixelMap();
    }
    public byte[,] GetFuzzyMap()
    {
        Console.WriteLine($"Size of fuzzymap is {_fuzzyMap.GridSize}");
        return Combine(_fuzzyMap, _sharpMap);
    }
    
    public List<int[]> GetConnectionMap()
    {
        return _connectionMap.GetPointsInt();
    }
    
    public enum debugType
    {
        WatchPixels,
        PlaceSpecificPixel,
        ListALlConnections
    }
    public void Debug(debugType debugType = debugType.WatchPixels)
    {
        Console.WriteLine("Starting Debugging, press space to continue after each step.");
        switch (debugType)
        {
            case debugType.WatchPixels:
                Console.Write("Watching pixels... ");
                _sharpMap.Initialize();
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine($"Starting Pixel Grid is");
                _sharpMap.DrawGrid();
                Console.WriteLine();
                Console.ReadKey();

                for (int j = 0; j < 5; j++)
                {
                    try { CreateNewPixel(tries:10); Console.Write(".");}
                    catch (Exception e) { Console.WriteLine(e); }
                    Console.ReadKey();
                }
        
                _sharpMap.DrawGrid(); 
                break;
            case debugType.PlaceSpecificPixel:
                Console.WriteLine($"Starting Pixel Grid is");
                _sharpMap.DrawGrid();
                Console.WriteLine();
                Console.WriteLine("Placing specific pixel.");
                Console.Write("X: ");
                int x = Convert.ToInt32(Console.ReadLine());
                Console.Write("Y: ");
                int y = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
                Console.WriteLine();
                
                Console.ReadKey();
                Console.WriteLine($"Placing Pixel at {x}, {y}");
                Coordinate p = new Coordinate(x, y);
                Coordinate dir = new Coordinate(0,0);
                bool canPlace = CheckPosition(p, dir);

                _sharpMap.AddPixel(p.x, p.y, 255);
                Console.ForegroundColor = canPlace ? ConsoleColor.Green : ConsoleColor.Red;
                _sharpMap.DrawGrid();
                _sharpMap.UpdatePixel(p.x, p.y, 0);
                Console.ResetColor();
                
                Console.ReadKey();
                Console.WriteLine($"This pixel can be placed: {canPlace}");
                break;
            case debugType.ListALlConnections:
                Console.Write("Listing connections... ");
                Console.ReadKey();
                List<int[]> conns = _connectionMap.GetPointsInt();
                foreach (int[] c in conns)
                {
                   Console.WriteLine($"Point: {c[0]},{c[1]} is connected to: {c[2]}, {c[3]}"); 
                }
                break;
    }
        
    }
}