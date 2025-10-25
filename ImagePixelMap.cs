namespace RidgeGenerator;
using SkiaSharp;

//An extention of the pixel map that contains image manipulation methods. (Interpolation, Convolution)
public class ImagePixelMap : PixelMap
{
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
        if (result > 255) return 255;
        return (byte)result;
    }
    
    //will produce a convoluted blurry version of the image. 
    public void Convolute()
    {
        byte[,] convolutedImage = new byte[GridSize, GridSize];
        double[,] kernalLarge = new double[5, 5]
        {  //circular kernal for a softer approach
            {   0  ,   0  ,1f/13f,  0   ,  0   },
            {   0  ,1f/13f,1f/13f,1f/13f,  0   },
            {1f/13f,1f/13f,1f/13f,1f/13f,1f/13f},
            {   0  ,1f/13f,1f/13f,1f/13f,  0   },
            {   0  ,   0  ,1f/13f,  0   ,  0   }
        };
        double[,] kernalSmall = new double[3, 3]
        {  //smaller circular kernal for a less precise approach
            {   0  ,1f/5f,  0   },
            {1f/5f,1f/5f,1f/5f},
            {   0  ,1f/5f,  0   }
        };
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                convolutedImage[x,y] = Convolve(kernalLarge,x,y);
            }
        }
        _pixelMap = convolutedImage;
    }
    
    //upscale the grid by linear interpolation
    public void UpscaleLinearInterpolation(int scale)
    {
        //covert pixelmap into a bitmap
        SKBitmap newBitmap = ConvertToImage.ByteArrayToImage(ConvertToImage.IntArrayToByteArray(_pixelMap));
        SKBitmap upscaledBitmap = new SKBitmap();
        //then upscale using linear interpolation from Skiasharp
        SKSamplingOptions samplingOptions = new SKSamplingOptions(filter:SKFilterMode.Linear);
        SKSizeI imageSize = new SKSizeI( GridSize * scale, GridSize * scale);
        upscaledBitmap = newBitmap.Resize(imageSize, samplingOptions);
        //we're doing it this was because I'm lazy - Sean
        
        _pixelMap = ConvertToImage.BitmapToByteArray(upscaledBitmap);
    }
    
    //double the size of the map by using Linear Interpolation, then add a small blur with convolution
    public void Upscale()
    {
        //upscale the map to twice it's size
        //linearly interpolate the values onto the bigger grid
        UpscaleLinearInterpolation(2);
        //perform a convolution on this image to blur it
        Convolute();
    }
}