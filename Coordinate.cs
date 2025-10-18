namespace RidgeGenerator;

public struct Coordinate(int x, int y, int weight = 255)
{
    public int x = x;
    public int y = y;
    public int weight = weight;

    public override string ToString()
    {
        return $"({x},{y})";
    }

    public static Coordinate operator +(Coordinate c1, Coordinate c2)
    {
        c1.x += c2.x; 
        c1.y += c2.y;
        return c1;
    }
    public static Coordinate operator -(Coordinate c1, Coordinate c2)
    {
        c1.x += c2.x; 
        c1.y += c2.y;
        return c1;
    }
    public static Coordinate operator *(Coordinate c, int a)
    {
        c.x *= a;
        c.y *= a;
        return c;
    }

    public static bool operator ==(Coordinate c1, Coordinate c2)
    {
        return c1.x == c2.x && c1.y == c2.y;
    }
    public static bool operator !=(Coordinate c1, Coordinate c2)
    {
        return c1.x != c2.x || c1.y != c2.y;
    }
}