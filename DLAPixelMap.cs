namespace RidgeGenerator;

//A class which represents a grid of pixels, performs DLA algorithm on those pixels.
public class DLAPixelMap : PixelMap
{
    public int Seed;
    private readonly Random _random;
    private readonly PixelConnectionMap _connectionMap = new PixelConnectionMap();
    
    //exposed variables
    public int SpawnBorder = 0;
    public float JiggleAmount = 0f;
        //does position check include diagonals?
        //density of pixels
    
    //constructors
    public DLAPixelMap(int Seed)
    {
        this.Seed = Seed;
        this._random = new Random(Seed);
    }

    //if seed is not speficied, get the seed from the system clock
    public DLAPixelMap()
    {
        this.Seed = System.Environment.TickCount;
        this._random = new Random(Seed);
    }
    
    //Make one random position max height
    public void CreateGrid()
    {
        Console.WriteLine("Creating grid"); //Generate a random "Seed" Pixel
        var posX = _random.Next(1, GridSize - 1); 
        var posY = _random.Next(1, GridSize - 1);
        Point seed = new Point(posX, posY, 255, null);
        //Add seed to the map and run DLA
        AddPixel(seed.x, seed.y, 1);
        _connectionMap.AddPoint(seed);
        Populate(height:50);             //DLA algorithm
        Upscale();              //Upscale
        //Upscale();
        //Upscale();
        //Populate(0.2);          //DLA algorithm
        MapConnectionMap();
        _connectionMap.AssignWeights();
        Console.WriteLine($"Created Starting block of {GridSize}x{GridSize}");
        //DrawGrid();
        
    }
    
    //this will move the pixel in a random direction and ensure it doesn't leave the bounds of the grid
    private Coordinate MoveInRandomDirection(Coordinate pos, out Coordinate direction)
    {
        int newX = 0, newY = 0;
        var maxX = GridSize-1;
        var maxY = GridSize-1;
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
        
        direction = new Coordinate(newX, newY, 0);
        return pos + direction;
    }
    
    // Add a new pixel to the grid, then move it in random directions until it is next to another pixel
    //returns the final position of the pixel
    private Coordinate CreateNewPixel(byte height = 1, int tries = 1000)
    {
        byte newPixel = height; //this will be an object in the future possibly?
        //spawn the pixel in a random position
        Coordinate newPosition = new Coordinate
        (
            _random.Next(GridSize - SpawnBorder),
            _random.Next(GridSize - SpawnBorder),
            newPixel
        );
        //Keep trying to add a new pixel
        int c = 0;
        while (GetPixel(newPosition.x, newPosition.y) > 0)
        {
            if (c == tries) { throw(new Exception("This grid is probably full or you are incredibly unlucky."));}
            newPosition.x = _random.Next(GridSize - SpawnBorder);
            newPosition.y = _random.Next(GridSize - SpawnBorder);
            c++;
        }
        
        Coordinate direction = new Coordinate(0,0, 0);//direction we just moved

        //Keep moving in random directions until we are next to another pixel, caps at c number of moves
        c = 0;
        while (!CheckPosition(newPosition, direction))
        {
            if (c == tries) { throw new Exception($"Timed out. Couldn't find a neighbor after {c} tries. Pixel's final coords are {newPosition.x}, {newPosition.y}"); }
            
            newPosition = MoveInRandomDirection(newPosition, out direction);
            c++;
        }
        AddPixel(newPosition.x, newPosition.y, newPixel);
        //Console.WriteLine($"Created new pixel of {newPosition.weight}");
        return newPosition;
    }

    private void StickTo(Coordinate pos, Coordinate neighbor)
    {
        _connectionMap.AddPoint(pos.x, pos.y, pos.weight, neighbor);
        //Console.WriteLine($"{pos} has stuck to {neighbor} and has a weight of {pos.weight}");
    }
    
    //check the (8|4) positions around the pixel to see if there are any neighbors
    //then connect to the neighbor it ran in to
    private bool CheckPosition(Coordinate p, Coordinate direction)
    {
        Coordinate neighbor;
        int currentSize = GridSize;
        
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
            if (positions[i,0] < 0 || positions[i,0] > (currentSize-1) || positions[i,1] < 0 || positions[i,1] > (currentSize-1)){continue;}//skip this iteration if we exit the bounds of the array

            int neighborWeight = GetPixel(positions[i, 0], positions[i, 1]);
            Coordinate neighboringPixel = new Coordinate(positions[i, 0], positions[i,1], neighborWeight);
            if (GetPixel(neighboringPixel.x, neighboringPixel.y) > 0) //check the value of the neighboring pixel
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
            //Console.WriteLine($"We found {neighbors.Count} neighbors! Us: {p}-{p.weight} is now connected to {neighbors[0]}-{neighbors[0].weight}");
            StickTo(p, neighbor);
            return true;
        }
        //we have no neighbors, move again until we find one.
        neighbor = p;//Returns the neighbor as ourselves
        return false;
        //IGNORE THIS -> OLD INFO. since the coordinates never enter the negative plane we can safely use this as a null value
    }

    //populate the sharp grid with pixels equal to density times the volume
    public void Populate(double density = 1.00, byte height = 1, int tries = 10000, int fails = 0)
    {
        int numOfPixelsToPlace = (int)((GridSize * GridSize)/2*density);
        if (numOfPixelsToPlace < GridSize * GridSize) {numOfPixelsToPlace += 1;}
        //for each change in the size of the grid, add pixels equal to twice the size of the grid
        for (int j = 0; j < numOfPixelsToPlace; j++)
        {
            try
            {
                CreateNewPixel(height, tries: tries);
            }
            catch (Exception e)
            {
                fails++;
                Console.Write("X " + e);
            }
        }
        Console.Write('\n');
    }

    public void CreateSinglePixel(byte height = 255)
    {
        CreateNewPixel(height);

        for (int y = 0; y < GridSize; y++)
        {
            Console.Write('\n');
            for (int x = 0; x < GridSize; x++)
            {
                Console.Write(GetPixel(x, y));
                Console.Write(", ");
            }
        }
    }
    
    //This will map the connection map into the pixel map
    private void MapConnectionMap()
    {
        //wipe the original points
        Clear();
        var points = _connectionMap.GetPoints();
        foreach (Point pixel in points)
        {
            if (pixel.Weight > 255) pixel.Weight = 255;
            AddPixel(pixel.x, pixel.y, (byte)pixel.Weight);
        }
        //Console.WriteLine($"The size of our ConnectionMap is {_connectionMap.Size}, and we placed {c} Pixels");
    }

    //double this size of the map by using the record of which pixels got stuck to which.
    public void Upscale()
    {
        //First Expand the grid to twice the size.
        ExpandGrid();
        //then make sure all the pixels in the connection grid are updated too.
        _connectionMap.UpscaleDouble(JiggleAmount);
        //finally map the connections onto the pixel grid
        //_connectionMap.AssignWeights();
        MapConnectionMap();
    }

    public PixelConnectionMap GetConnectionMap()
    {
        return _connectionMap;
    }
    
        public enum DebugType
    {
        WatchPixels,
        PlaceSpecificPixel,
        ListALlConnections,
        CreateSmallGrid
    }
    public void Debug(DebugType debugType = DebugType.WatchPixels)
    {
        Console.WriteLine("Starting Debugging, press space to continue after each step.");
        switch (debugType)
        {
            case DebugType.WatchPixels:
                Console.Write("Watching pixels... ");
                Clear();
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine($"Starting Pixel Grid is");
                DrawGrid();
                Console.WriteLine();
                Console.ReadKey();

                for (int j = 0; j < 5; j++)
                {
                    try { CreateNewPixel(tries:10); Console.Write(".");}
                    catch (Exception e) { Console.WriteLine(e); }
                    Console.ReadKey();
                }
        
                DrawGrid(); 
                break;
            case DebugType.PlaceSpecificPixel:
                Console.WriteLine($"Starting Pixel Grid is");
                DrawGrid();
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
                Coordinate p = new Coordinate(x, y, 255);
                Coordinate dir = new Coordinate(0,0, 255);
                bool canPlace = CheckPosition(p, dir);

                AddPixel(p.x, p.y, 255);
                Console.ForegroundColor = canPlace ? ConsoleColor.Green : ConsoleColor.Red;
                DrawGrid();
                UpdatePixel(p.x, p.y, 0);
                Console.ResetColor();
                
                Console.ReadKey();
                Console.WriteLine($"This pixel can be placed: {canPlace}");
                break;
            case DebugType.ListALlConnections:
                Console.Write("Listing connections... ");
                Console.ReadKey();
                List<int[]> conns = _connectionMap.GetPointsInt();
                foreach (int[] c in conns)
                {
                   Console.WriteLine($"Point: {c[0]},{c[1]} is connected to: {c[2]}, {c[3]}"); 
                }
                break;
            case DebugType.CreateSmallGrid:
                Console.Write("Creating Small Grid of 8x8..\n\n");
                DrawGrid();
                _connectionMap.DrawMap();
                break;
    }
        
    }
}