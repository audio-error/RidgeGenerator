using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace RidgeGenerator;

public class Point
{
    public int x;
    public int y;
    public int Weight;
    public Point? ConnectedPoint;
    
    public Point(int x, int y, int weight = 255, Point? connectedPoint = null)
    {
        this.x = x;
        this.y = y;
        this.Weight = weight;
        this.ConnectedPoint = connectedPoint;
    }

    //Returns the midpoint between ourselves and our connected point.
    //returned point is the average weight of us and our connection, and will be connected to our connected point.
    public Point GetMidpoint()
    {
        if (this.ConnectedPoint is null)
        {
            throw new Exception("Can not get midpoint when connected point is null.");
        }
        int newX = (x + ConnectedPoint.x) / 2;
        int newY = (y + ConnectedPoint.y) / 2;
        //Console.WriteLine($"{Weight} - {(Weight + ConnectedPoint.Weight) / 2} - {ConnectedPoint.Weight}");
        int newWeight = (Weight + ConnectedPoint.Weight) / 2;
        Point midPoint = new Point(newX, newY, newWeight,null);
        //Console.WriteLine($"New Midpoint: {this},{midPoint},{ConnectedPoint}");
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
        return p1.x == p2.x && p1.y == p2.y;
    }
    public static bool operator !=(Point? p1, Point? p2)
    {
        if (p1 is null && p2 is null)
        {
            return false;
        }
        return p1.x != p2.x && p1.y != p2.y;
    }

    //Will add the x and y together but leave the weight and connected point alone
    public static Point operator +(Point p1, Point p2)
    {
        int newx = p1.x + p2.x;
        int newy = p1.y + p2.y;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //Will subtract the x and y but leave the weight and connected point alone
    public static Point operator -(Point p1, Point p2)
    {
        int newx = p1.x - p2.x;
        int newy = p1.y - p2.y;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
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
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //Will subtract the x and y but leave the weight and connected point alone
    public static Point operator -(Point p1, Coordinate p2)
    {
        int newx = p1.x - p2.x;
        int newy = p1.y - p2.y;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
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
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    //will divide the x and y by the integer value (truncated) and leave the weight and connected point alone.
    public static Point operator /(Point p1, int a)
    {
        int newx = p1.x / a;
        int newy = p1.y / a;
        return new Point(newx, newy, p1.Weight, p1.ConnectedPoint);
    }
    
    
}