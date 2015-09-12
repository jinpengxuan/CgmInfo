using System.Windows;
using CgmInfoGui.Visuals;

namespace CgmInfoGui.Traversal
{
    public class GraphicalElementContext
    {
        public TextVisual LastText { get; set; }
        public VisualRoot Visuals { get; } = new VisualRoot();

        public void Add(VisualBase visual)
        {
            Visuals.Add(visual);
        }
        public void SetExtent(Point lowerLeft, Point upperRight)
        {
            Visuals.VdcExtent = new Rect(lowerLeft, upperRight);
            Visuals.DirectionX = lowerLeft.X <= upperRight.X ? 1.0 : -1.0;
            Visuals.DirectionY = lowerLeft.Y <= upperRight.Y ? 1.0 : -1.0;
        }
        public void IncreaseBounds(Point point)
        {
            IncreaseBounds(new Rect(point, new Size(1, 1)));
        }
        public void IncreaseBounds(Rect rect)
        {
            if (Visuals.GeometryExtent.IsEmpty)
            {
                Visuals.GeometryExtent = rect;
            }
            else
            {
                Visuals.GeometryExtent = Rect.Union(Visuals.GeometryExtent, rect);
            }
        }
    }
}
