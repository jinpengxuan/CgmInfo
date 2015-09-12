using System;
using System.Windows;
using System.Windows.Media;

namespace CgmInfoGui.Visuals
{
    public abstract class VisualBase
    {
        protected internal abstract void DrawTo(DrawingContext drawingContext, VisualContext visualContext);
        private static Pen Black;
        protected static Pen GetBlack()
        {
            if (Black == null)
            {
                Black = new Pen(Brushes.Black, 1);
                Black.Freeze();
            }
            return Black;
        }

        public static double RadiansToDegrees(double rad)
        {
            return 180 / Math.PI * rad;
        }
        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2.0) + Math.Pow(p2.Y - p1.Y, 2.0));
        }
    }
}
