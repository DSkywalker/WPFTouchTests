using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchDeviceAutoCheck
{

    public class TouchCursor
    {
        public int ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        private static Random r = new Random();

        public TouchCursor(int id, double x, double y, double w, double h)
        {
            ID = id;
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public TouchCursor(int id, double x, double y)
        {
            ID = id;
            X = x;
            Y = y;
            Width = 20;
            Height = 20;
        }

        public static TouchCursor GetRandomCursor(int id, double w, double h)
        {
            return new TouchCursor(id, r.NextDouble() * w, r.NextDouble() * h);
        }
    }

}
