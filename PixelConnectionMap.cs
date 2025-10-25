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
    public void AddPoint(int x, int y, int weight, Coordinate neighbor)
    {
        //Console.WriteLine($"Received ({x},{y})-{neighbor}");
        //loop through all the points to find the matching neighbor
        foreach (Point point in _points)
        {
            //Console.WriteLine($"Checking if {neighbor} = {point}");
            if (point == neighbor)
            {
                //Console.WriteLine($"Yes, they are the same");
                //create a new point object and make it's neighbor the point we just found
                Point newPoint = new Point(x, y, weight, point);
                _points.Add(newPoint);
                //Console.WriteLine($"Adding {newPoint}");
                Size++;
                return;
            }
        }
        throw new Exception("We Found no matching neighbor point to connect to.");
    }
    public void AddPoint(Point p)
    {
        //Console.WriteLine($"Received {p}, but it has no neighbors");
        Size++;
        _points.Add(p);
    }

    //upscales by 2 times
    //Todo: jiggle midpoints a small amount to reduce the artificialness of the ridges
    public void UpscaleDouble(float jiggleAmount)
    {
        Console.WriteLine($"Calling Upscale");
        List<Point> newPoints =  new List<Point>();
        var alreadyAdded = new List<long>();   // tracks originals that are already scaled
        Size = 0;
        foreach (Point originalPoint in _points)
        {
            //Stretch all the points by 2
            var upscaledPoint = originalPoint * 2;
            
            // If this original was already added as a neighbour, skip it.
            if (alreadyAdded.Contains(upscaledPoint.Id))
                continue;

            //now we need to fill in the gaps with new points
            if (originalPoint.ConnectedPoint != null)//skip if we aren't connected to any points (we will generate no midpoints)
            {
                
                //upscale the neighbor
                var scaledNeighbor = originalPoint.ConnectedPoint * 2;
                //find the midpoint of us and the connected point
                var midPoint = upscaledPoint.GetMidpoint(scaledNeighbor);
                midPoint.Weight = upscaledPoint.Weight;
                //Console.WriteLine($"{upscaledPoint.Weight} | {midPoint.Weight} | {scaledNeighbor.Weight}");
                
                //update our connections
                upscaledPoint.ConnectedPoint = midPoint; //connect us to the midpoint
                midPoint.ConnectedPoint = scaledNeighbor; //connect this point to our previously connected point
                
                newPoints.Add(midPoint);
                newPoints.Add(scaledNeighbor);
                Size += 2;
                
                // Mark neighbour as “already added”
                alreadyAdded.Add(scaledNeighbor.Id);
            }
            
            newPoints.Add(upscaledPoint);
            Size++;
        }
        for (int i = newPoints.Count - 1; i >= 0; i--)
        {
            newPoints[i].Weight = 50;
        }
        Console.WriteLine($"Upscaled {newPoints.Count} points");
        _points = newPoints;
        
        // Verify no duplicate original references exist in the new list
        var dupCheck = _points
            .GroupBy(p => p.Id)          // group by reference
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        

        Console.WriteLine(dupCheck.Count == 0
            ? "No duplicate objects in the upscaled list."
            : $"Found {dupCheck.Count} duplicate references!");
        
    }

    //loops through the tree and fixes heights.
    public void AssignWeights()
    {

        var roots = _points.GetRootNodes().ToArray();
        int index = 0;
        foreach (var root in roots)   // roots = points with no ConnectedPoint or that are entry points
        {
            index = _points.IndexOf(root);
            _points[index].Weight = 255;
            Console.WriteLine($"Assigning {root} to {_points[index].Weight}");
            //PropagateWeight(root);
        }
        //PropagateWeight(roots[1]);
    }

    private void PropagateWeight(Point node)
    {
        if (node == null) return;          // base case – reached the end of the chain
        Console.WriteLine($"Propagating weight {node+1}");
        node.Weight = 255;                 // highlight current point
        PropagateWeight(node.ConnectedPoint); 
    }
    
    //Depreciated
    /*draws the map as a grid to the console*/
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

        foreach (Point point in _points)
        {
            map[point.x, point.y] = point.Weight;
        }
        
        Console.WriteLine("Drawing {0} points", _points.Count);
        Console.WriteLine("Drawing Grid of size: {0} by {1}", maxX, maxY);
        for (int i = 0; i < map.GetLength(1); i++)
        {
            for (int j = 0; j < map.GetLength(0); j++)
            {
                //draw the point
                if (map[j, i] != 0)
                {
                    //Console.Write("o");

                    Console.Write(map[j, i]);
                    Console.Write('|');
                }
                else {Console.Write("  ");}
            }

            Console.Write('\n');
        }
    }
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