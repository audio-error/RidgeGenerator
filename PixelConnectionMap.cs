using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RidgeGenerator;

//This class records which pixels got stuck to which, and can be redrawn as lines.
//This is essential for upscaling the image with zero blur
public class PixelConnectionMap
{
    //simple structure for holding an x and y value
    //point which is connected to one other point. We only keep track of the one other point that this point ran into during DLA

    private List<Point> _points;
    
    public int Size;

    public PixelConnectionMap(int size = 0)
    {
       this.Size = size;
        _points = new List<Point>();
    }

    //returns a list of coordinates. First two are the point, second two are the coords of it's connected point
    public List<int[]> GetPointsInt()
    {
        List<int[]> points = [];
        foreach (Point point in _points)
        {
            int[] cPoint;
            if (point.ConnectedPoint is null) cPoint = [point.x, point.y];
            else cPoint = [point.ConnectedPoint.x, point.ConnectedPoint.y];
            points.Add([point.x, point.y, cPoint[0], cPoint[1]]);
        }
        return points;
    }

    public List<Point> GetPoints()
    {
        return _points;
    }
    
    //when adding a point we must pass the point it is connected too
    public void AddPoint(int x, int y, int connectedX, int connectedY, int weight)
    {
        //Console.WriteLine($"Connecting {x},{y} to {connectedX},{connectedY}");
        Point point = new Point(x, y, weight);
        if (x != connectedX || y != connectedY)
        {
            point.ConnectedPoint = new Point(connectedX, connectedY);
        }
        //Console.WriteLine($"Adding: {point}");
        _points.Add(point);
        Size++;
    }
    public void AddPoint(Point p)
    {
        //Console.WriteLine($"Adding: {p}");
        Size++;
        _points.Add(p);
    }

    //upscales by 2 times
    //Todo: jiggle midpoints a small amount to reduce the artificialness of the ridges
    public void UpscaleDouble(int amount)
    {
        List<Point> newPoints =  new List<Point>();
        Size = 0;
        foreach (Point point in _points)
        {
            
            //Stretch all the points by 2
            Point upscaledPoint = point * 2;
            
            //Console.WriteLine($"Upscaling Point: {point} -> {upscaledPoint}: Weight: {upscaledPoint.Weight}");
            newPoints.Add(upscaledPoint); Size++;
            //continue;
            //now we need to fill in the gaps with new points
            if (upscaledPoint.ConnectedPoint is not null)//skip if we aren't connected to any points (we will generate no midpoints)
            {
                upscaledPoint.ConnectedPoint *= 2;
                Point midPoint = upscaledPoint.GetMidpoint();//find the midpoint of us and the connected point
                //Console.WriteLine($"{upscaledPoint}-{midPoint}-{upscaledPoint.ConnectedPoint}");
                midPoint.ConnectedPoint = upscaledPoint.ConnectedPoint; //connect this point to our previously connected point
                upscaledPoint.ConnectedPoint = midPoint; //connect us to the midpoint
                newPoints.Add(midPoint);
                Size++;
            }
            //Console.WriteLine($"Point: {upscaledPoint} -> {upscaledPoint.ConnectedPoint}");
            
        }
        _points = newPoints;
    }
    
    //Depreciated
    /*draws the map as a grid to the console
    public void DrawMap()
    {
        //get the size of the grid
        int maxX = 0;
        int maxY = 0;
        List<int> totalX = new List<int>();
        List<int> totalY = new List<int>();
        for (int i = 0; i < _points.Count; i++)
        {
            totalX.Add(_points[i].x);
            totalY.Add(_points[i].y);
        }
        maxX = totalX.Max()+1;
        maxY = totalY.Max()+1;
        
        //create grid and set all values to 0
        int[,] map = new int[maxX, maxY];
        map.Initialize();
        
        //add the line connections to the grid
        int c = _points.Count;
        for (int x = 0; x < c; x++)
        {
            //continue; //debug removed
            var p1 = _points[x];
            // negative connections are null.
            var cPoint = p1.connectedPoint;
            if (cPoint is not { x: < 0, y: < 0 })
            {
                var line = GetLinePoints(p1.ToCoordintate(), cPoint);
                //Console.WriteLine($"Line Between: {p1.ToCoordintate()}:{cPoint}. Line is {line.Count} Points Long");
                foreach (var p2 in line)
                {
                    _points.Add(p2);
                }
            }
        }
        
        //add the points to the grid
        foreach (Point point in _points)
        {
            if (!point.isLine)
            {
                //DEBUG:
                //Console.WriteLine($"Map size is {maxX},{maxY}");
                //Console.WriteLine($"Adding {point} To the Map");
                map[point.x, point.y] = 1;
            }
            else
            {
                //DEBUG: Console.WriteLine("adding line {0},{1}", point.x, point.y);
                map[point.x, point.y] = 2;
            }
        }
        
        Console.WriteLine("Drawing {0} points", _points.Count);
        Console.WriteLine("Drawing Grid of size: {0} by {1}", maxX, maxY);
        for (int i = 0; i < map.GetLength(1); i++)
        {
            for (int j = 0; j < map.GetLength(0); j++)
            {
                //draw the point
                if (map[j, i] == 1) {Console.Write("o");}
                //draw the lines between the points
                if (map[j, i] == 2)
                {
                    Console.Write("*");
                }
                else {Console.Write(" ");}
            }

            Console.Write('\n');
        }
    }
    */
    //Depreciated
    /*
    private static List<Point> GetLinePoints(Coordintate start, Coordintate end)
    {
        List<Point> points = new List<Point>();

        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);
        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (start.x == end.x && start.y == end.y)
                break;

            int err2 = err * 2;
            if (err2 > -dy)
            {
                err -= dy;
                start.x += sx;
            }
            if (err2 < dx)
            {
                err += dx;
                start.y += sy;
            }

            if (!(end.x == start.x && end.y == start.y))
            {
                points.Add(new Point(Math.Abs(start.x), Math.Abs(start.y)));
            }
        }

        return points;
    }
    */
}