using System.Drawing;

namespace MaximTechnology;

public static class RouteBuilder
{
    public static Route Build(Point from, Point to)
    {
        var path = new List<Point>(Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y) + 1) { from };

        var x = from.X;
        var y = from.Y;

        var stepX = Math.Sign(to.X - x);
        while (x != to.X)
        {
            x += stepX;
            path.Add(new Point(x, y));
        }

        var stepY = Math.Sign(to.Y - y);
        while (y != to.Y)
        {
            y += stepY;
            path.Add(new Point(x, y));
        }

        return new Route(path);
    }
}

public record Route(List<Point> Points);