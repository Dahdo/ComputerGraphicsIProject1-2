using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComputerGraphicsIProject
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Color PixelColor { get; set; }

        public Point(int x, int y, Color color)
        {
            X = x;
            Y = y;
            PixelColor = color;
        }
    }

    public abstract class Shape
    {
        public List<Point> Points { get; set; }
        public Color PixelColor { get; set; }
        public void Draw(WriteableBitmap imageCanvasBitmap)
        {
            // source: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?view=windowsdesktop-8.0
            System.Windows.Media.Color color;

            try
            {
                // Reserve the back buffer for updates.
                imageCanvasBitmap.Lock();

                unsafe
                {

                    foreach (Point p in this.Points)
                    {
                        // Get a pointer to the back buffer.
                        IntPtr pBackBuffer = imageCanvasBitmap.BackBuffer;

                        int column = p.X;
                        int row = p.Y;
                        color = p.PixelColor;
                        // Find the address of the pixel to draw.
                        pBackBuffer += row * imageCanvasBitmap.BackBufferStride;
                        pBackBuffer += column * 3;

                        // Compute the pixel's color.
                        int color_data = color.R << 16; // R
                        color_data |= color.G << 8;   // G
                        color_data |= color.B << 0;   // B

                        try
                        {
                            //Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color_data;
                            // Specify the area of the bitmap that changed.
                            imageCanvasBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.Message);
                            this.Points.Clear();
                            break;
                        }
                    }

                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                imageCanvasBitmap.Unlock();
            }
        }

        public void ChangeColor(Color color)
        {
            foreach(Point p in this.Points)
                p.PixelColor = color;
            this.PixelColor = color;
        }
    }

    public class Line : Shape
    {
        public Point startPoint {  get; set; }
        public Point endPoint { get; set; }
        public int Thickness { get; set; }

        public Line(int thickness = 1)
        {
            Points = new List<Point>();
            Thickness = thickness;
            PixelColor = Colors.Yellow;
            startPoint = new Point(-1, -1, PixelColor);
            endPoint = new Point(-1, -1, PixelColor);
        }

        public void CalculateMidpointLineAlgorithm()
        {

            // Source: https://www.geeksforgeeks.org/mid-point-line-generation-algorithm/

            int X1 = this.startPoint.X;
            int X2 = this.endPoint.X;
            int Y1 = this.startPoint.Y;
            int Y2 = this.endPoint.Y;

            int dx = Math.Abs(X2 - X1);
            int dy = Math.Abs(Y2 - Y1);

            if (X1 > X2)
            {
                X1 = -X1;
                X2 = -X2;
            }
            if (Y1 > Y2)
            {
                Y1 = -Y1;
                Y2 = -Y2;
            }



            if (dy <= dx)
            {
                int d = dy - (dx / 2);
                int x = X1, y = Y1;

                this.Points.Add(new Point(Math.Abs(x), Math.Abs(y), PixelColor));
                while (x < X2)
                {
                    x++;
                    // choose E
                    if (d < 0)
                        d = d + dy;

                    // choose NE
                    else
                    {
                        d += (dy - dx);
                        y++;
                    }
                    this.Points.Add(new Point(Math.Abs(x), Math.Abs(y), PixelColor));
                }
            }
            else if (dx < dy)
            {
                int d = dx - (dy / 2);
                int x = X1, y = Y1;

                this.Points.Add(new Point(Math.Abs(x), Math.Abs(y), PixelColor));

                while (y < Y2)
                {
                    y++;
                    // Choose E

                    if (d < 0)
                        d = d + dx;

                    // Choose NE
                    else
                    {
                        d += (dx - dy);
                        x++;
                    }

                    this.Points.Add(new Point(Math.Abs(x), Math.Abs(y), PixelColor));
                }
            }
        }

    }


    public class Circle : Shape
    {
        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
        public int Thickness { get; set; }

        public Circle(int thickness = 1)
        {
            Points = new List<Point>();
            Thickness = thickness;
            PixelColor = Colors.Yellow;
            startPoint = new Point(-1, -1, PixelColor);
            endPoint = new Point(-1, -1, PixelColor);
        }

        public void CalculateMidpointCircleAlgorithm()
        {
            int radius = getRadius();
            int d = 1 - radius;
            int x = 0;
            int y = radius;
            while (y > x)
            {
                addPoints(x, y);

                if (d < 0)
                    d += 2 * x + 3;
                else
                {
                    d += 2 * (x - y) + 5;
                    --y;
                }
                ++x;
            }
        }

        private int getRadius()
        {
            return (int)Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));
        }

        private void addPoints(int x, int y)
        {
            this.Points.Add(new Point(startPoint.X + x, startPoint.Y + y, PixelColor));
            this.Points.Add(new Point(startPoint.X + y, startPoint.Y + x, PixelColor));
            this.Points.Add(new Point(startPoint.X - x, startPoint.Y + y, PixelColor));
            this.Points.Add(new Point(startPoint.X - y, startPoint.Y + x, PixelColor));
            this.Points.Add(new Point(startPoint.X + x, startPoint.Y - y, PixelColor));
            this.Points.Add(new Point(startPoint.X + y, startPoint.Y - x, PixelColor));
            this.Points.Add(new Point(startPoint.X - x, startPoint.Y - y, PixelColor));
            this.Points.Add(new Point(startPoint.X - y, startPoint.Y - x, PixelColor));
        }
    }

    public class Polygon:Shape
    {
        private Point _nextPoint;
        public Point nextPoint { get => _nextPoint; 
            set
            {
                if (prevPoint == null && _nextPoint.X != -1)
                    prevPoint = _nextPoint; // If prevPoint was never initialized (the frist edge of poygon)
                _nextPoint = value;
            }
        }
        private Point? prevPoint;

        private Line? line;
        public int Thickness { get; set; }

        public Polygon(int thickness = 1)
        {
            this.Points = new List<Point>();
            this.Thickness = thickness;
            this.PixelColor = Colors.Yellow;
            _nextPoint = new Point(-1, -1, PixelColor);
        }

        public void CalculatePolygonPoints(WriteableBitmap imageCanvasBitmap)
        {
            line = new Line();
            // Assimulate line to our Polygon settings
            line.PixelColor = this.PixelColor;
            line.Thickness = Thickness;

            // Set points
            line.startPoint = prevPoint!;
            if (lastEdge(nextPoint))
                line.endPoint = Points.First();
            else
                line.endPoint = nextPoint;

            line.CalculateMidpointLineAlgorithm();
            line.Draw(imageCanvasBitmap);
            prevPoint = line.Points.Last(); // Keep the previous edge's endpoint

            Points.AddRange(line.Points); // Possible redrawing duplication of one pixel on edge joints but not an issue
        }

        public bool lastEdge(Point point)
        {
            if(Points.Count >  0)
                return getDistance(Points.First(), point) <= 10;
            return false;
        }

        private int getDistance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}


