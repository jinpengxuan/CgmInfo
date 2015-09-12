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
    }
}
