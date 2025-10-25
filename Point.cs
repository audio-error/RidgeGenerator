using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace RidgeGenerator;

public class Point(int x, int y, int weight, Point? connectedPoint = null)
{
    public int x = x;
    public int y = y;
    public long Id => ToId(x,y);
    public int Weight = weight;
    public Point? ConnectedPoint = connectedPoint;

    // Convert (x, y) → unique id
    public static long ToId(int x, int y)
    {
        //Promote to long to avoid overflow for large inputs
        long sum = (long)x + y;
        return (sum * (sum + 1) / 2) + y;
    }
    //Returns the midpoint between ourselves and another point.
    //returned point is the average weight of us and our connection, and will be connected to our connected point.
    public Point GetMidpoint(Point other)
    {
        int newX = (x + other.x) / 2;
        int newY = (y + other.y) / 2;
        int newWeight = (Weight + other.Weight) / 2;
        Point midPoint = new Point(newX, newY, newWeight,null);
        return midPoint;
    }
    
    public override string ToString()
    { 
        return $"({x},{y})";
    }
    
    
    public static bool operator ==(Point? p1, Point? p2)
    {
        if (p1 is null && p2 is null)
        {
            return true;
        }
        if (p1 is null || p2 is null)
        {
            return false;
        }
        return p1.x == p2.x && p1.y == p2.y;
    }
    public static bool operator !=(Point? p1, Point? p2)
    {
        if (p1 is null && p2 is null)
        {
            return false;
        }
        if (p1 is null || p2 is null)
        {
            return true;
        }
        return p1.x != p2.x && p1.y != p2.y;
    }
    public static bool operator ==(Point? p1, Coordinate p2)
    {
        if (p1 is null)
        {
            return false;
        }
        return p1.x == p2.x && p1.y == p2.y;
    }
    public static bool operator !=(Point? p1, Coordinate p2)
    {
        if (p1 is null)
        {
            return true;
        }
        return p1.x != p2.x && p1.y != p2.y;
    }

    //Will add the x and y together but leave the weight and connected point alone
    public static Point operator +(Point p1, Point p2)
    {
        int newx = p1.x + p2.x;
        int newy = p1.y + p2.y;
        int newWeight = p1.Weight + p2.Weight;
        return new Point(newx, newy, newWeight, p1.ConnectedPoint);
    }
    //Will subtract the x and y but leave the weight and connected point alone
    public static Point operator -(Point p1, Point p2)
    {
        int newx = p1.x - p2.x;
        int newy = p1.y - p2.y;
        int newWeight = p1.Weight - p2.Weight;
        return new Point(newx, newy, newWeight, p1.ConnectedPoint);
    }
    //Will add the x and y together but leave the weight and connected point alone
    public static Point operator +(Point p1, int a)
    {
        int newx = p1.x + a;
        int newy = p1.y + a;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //Will subtract the x and y but leave the weight and connected point alone
    public static Point operator -(Point p1, int a)
    {
        int newx = p1.x - a;
        int newy = p1.y - a;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //Will add the x and y together but leave the weight and connected point alone
    public static Point operator +(Point p1, Coordinate p2)
    {
        int newx = p1.x + p2.x;
        int newy = p1.y + p2.y;
        int newWeight = p1.Weight + p2.weight;
        return new Point(newx, newy, newWeight, p1.ConnectedPoint);
    }
    //Will subtract the x and y but leave the weight and connected point alone
    public static Point operator -(Point p1, Coordinate p2)
    {
        int newx = p1.x - p2.x;
        int newy = p1.y - p2.y;
        int newWeight = p1.Weight - p2.weight;
        return new Point(newx, newy, newWeight, p1.ConnectedPoint);
    }
    //will multiply the x and y by the integer value and leave the weight and connected point alone.
    public static Point operator *(Point p1, int a)
    {
        int newx = p1.x * a;
        int newy = p1.y * a;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //will multiply the x and y of the points together
    public static Point operator *(Point p1, Point p2)
    {
        int newx = p1.x * p2.x;
        int newy = p1.y * p2.y;
        int newWeight = p1.Weight + p2.Weight;
        return new Point(newx, newy, newWeight, p1.ConnectedPoint);
    }
    //will divide the x and y by the integer value (truncated) and leave the weight and connected point alone.
    public static Point operator /(Point p1, int a)
    {
        int newx = p1.x / a;
        int newy = p1.y / a;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
}


//Curtesy of ChatGPT.
//Gets the leaf nodes of the tree.
public static class PointExtensions
{
    // Returns the point(s) that start a chain (no incoming edges)
    public static IEnumerable<Point> GetRootNodes(this IEnumerable<Point> points)
    {
        //IDs that are referenced by other points (incoming edges)
        var incomingIds = new HashSet<long>();
        foreach (var p in points)
        {
            if (p.ConnectedPoint != null)
                incomingIds.Add(p.ConnectedPoint.Id);
        }

        //Roots are points whose Id is NOT in the incoming set
        return points.Where(p => !incomingIds.Contains(p.Id));
    }
}