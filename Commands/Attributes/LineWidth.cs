using CgmInfo.Traversal;

namespace CgmInfo.Commands.Attributes
{
    public class LineWidth : Command
    {
        public LineWidth(double width)
            : base(5, 3)
        {
            Width = width;
        }

        public double Width { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptAttributeLineWidth(this, parameter);
        }
    }
}
