using System;
using System.Collections.Generic;
using System.Linq;
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

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public abstract class Shape
    {
        public List<Point> Points { get; set; }
        public Color PiexelColor { get; set; }
        public void Draw(WriteableBitmap imageCanvasBitmap)
        {
            // source: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?view=windowsdesktop-8.0
            System.Windows.Media.Color color = this.PiexelColor;

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
            PiexelColor = Colors.Yellow;
            startPoint = new Point(-1, -1);
            endPoint = new Point(-1, -1);
        }

        public void CalculateMidpointLineAlgorithm1()
        {
            int dx = this.endPoint.X - this.startPoint.X;
            int dy = this.endPoint.Y - this.startPoint.Y;
            int x = this.startPoint.X;
            int y = this.startPoint.Y;

            int dx1 = Math.Abs(dx);
            int dy1 = Math.Abs(dy);
            int px = 2 * dy1 - dx1;
            int py = 2 * dx1 - dy1;

            int xe, ye;

            if (dy1 <= dx1)
            {
                if (dx >= 0)
                {
                    xe = this.endPoint.X;
                    ye = this.endPoint.Y;
                }
                else
                {
                    xe = this.startPoint.X;
                    ye = this.startPoint.Y;
                    this.startPoint = this.endPoint; ;
                }

                while (x < xe)
                {
                    this.Points.Add(new Point(x, y));
                    x++;
                    if (px < 0)
                    {
                        px = px + 2 * dy1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            y++;
                        }
                        else
                        {
                            y--;
                        }
                        px = px + 2 * (dy1 - dx1);
                    }
                }
            }
            else
            {
                if (dy >= 0)
                {
                    xe = this.endPoint.X;
                    ye = this.endPoint.Y;
                }
                else
                {
                    xe = this.startPoint.X;
                    ye = this.startPoint.Y;
                    this.startPoint = this.startPoint;
                }

                while (y < ye)
                {
                    this.Points.Add(new Point(x, y));
                    y++;
                    if (py <= 0)
                    {
                        py = py + 2 * dx1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            x++;
                        }
                        else
                        {
                            x--;
                        }
                        py = py + 2 * (dx1 - dy1);
                    }
                }
            }
        }



        public void CalculateMidpointLineAlgorithm()
        {

            // Source: https://www.geeksforgeeks.org/mid-point-line-generation-algorithm/

            int X1 = this.startPoint.X;
            int X2 = this.endPoint.X;
            int Y1 = this.startPoint.Y;
            int Y2 = this.endPoint.Y;

            int dx = X2 - X1;
            int dy = Y2 - Y1;

            if (dy <= dx)
            {
                int d = dy - (dx / 2);
                int x = X1, y = Y1;

                this.Points.Add(new Point(x, y));
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
                    this.Points.Add(new Point(x, y));
                }
            }
            else if (dx < dy)
            {
                int d = dx - (dy / 2);
                int x = X1, y = Y1;

                this.Points.Add(new Point(x, y));

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

                    this.Points.Add(new Point(x, y));
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
            PiexelColor = Colors.Yellow;
            startPoint = new Point(-1, -1);
            endPoint = new Point(-1, -1);
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
            this.Points.Add(new Point(startPoint.X + x, startPoint.Y + y));
            this.Points.Add(new Point(startPoint.X + y, startPoint.Y + x));
            this.Points.Add(new Point(startPoint.X - x, startPoint.Y + y));
            this.Points.Add(new Point(startPoint.X - y, startPoint.Y + x));
            this.Points.Add(new Point(startPoint.X + x, startPoint.Y - y));
            this.Points.Add(new Point(startPoint.X + y, startPoint.Y - x));
            this.Points.Add(new Point(startPoint.X - x, startPoint.Y - y));
            this.Points.Add(new Point(startPoint.X - y, startPoint.Y - x));
        }
    }

}


