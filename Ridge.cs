using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;

namespace RidgeGenerator;

//This is a simple DLA Algorithm that combines two upscaling techniques to make the image look like mountains
public class Ridge
{
    public int targetSize;
    //private PixelGrid _pixelGrid; //grid object, stores pixel data
    private DLAPixelMap _sharpMap; //the pixel map we apply brownian fractals to.
    private ImagePixelMap _fuzzyMap = new ImagePixelMap(); //the pixel map we blur and add our sharp map too to create` a height map
    
    //creates a square grid of pixels of length size. Initialises with one random pixel, uses default seed
    //creates an object to store which pixels get stuck to what
    public Ridge(int size)
    {
        this.targetSize = size;
        this._sharpMap = new DLAPixelMap();
        _CreateGrid();
    }

    //creates a square grid of pixels of length size. Initialises with one random pixel, uses specified seed for randomness
    public Ridge(int size, int seed)
    {
        this.targetSize = size;
        this._sharpMap = new DLAPixelMap(seed);
        _CreateGrid();
    }

    //This is the initilaiser for our Ridgelines
    //It will create two versions of our map to control how we add soft and crisp detail
    private void _CreateGrid()
    {
        //Initialise the DLA map, this will create a starter for us to work with.
        _sharpMap.CreateGrid();
        
        //Now we want to create two versions of this map.
        //The original version will be our "Sharp" map, we will use this to add new detail and
        //keep track of which pixels got stuck to which. It is the ridge lines of our mountains.
        //The second version is our "Fuzzy" map, we will upscale and blur this image to create the slopes of the mountains
        
        _fuzzyMap.SetPixelMap(_sharpMap.GetPixelMap());//create a copy of the starter map
        
        _fuzzyMap.AddPixelMap(_sharpMap); //Combine our slopes with our ridges
    }

    //this will run an iteration of the algorithm.
    //upscales crisp and upscales fuzzy, then combines them to produce a heightmap
    public void Iterate(int iterations)
    {
        //When we construct this object we create a small starter seed.
        //This is then split into two version, a Sharp version which we use to add more detail,
        //And a Fuzzy version which we add our new details too and create the softer slopes of the mountain.

        for (int i = 0; i < iterations; i++)
        {
            //First Upscale our crisp image
            _sharpMap.Upscale();
            //Then add some extra detail with DLA
            _sharpMap.Populate(0.2);
            
            //Then upscale our Fuzzy image so it is the same size
            _fuzzyMap.Upscale();
            //This will enlarge the image and also apply a small blur
            //Finally add our ridge lines to the fuzzy image
            _fuzzyMap.AddPixelMap(_sharpMap);
        }
        //perform one final convolution to soften the final output
        //_fuzzyMap.Convolute();
    }

    public byte[,] GetSharpMap()
    {
        return _sharpMap.GetPixelMap();
    }
    public List<Point> GetConnectionMap()
    {
        return _sharpMap.GetConnectionMap().GetPoints();
    }
    public byte[,] GetFuzzyMap()
    {
        return _fuzzyMap.GetPixelMap();
    }
    

}