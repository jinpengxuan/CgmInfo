using CgmInfo.Commands.Enums;
using CgmInfo.Traversal;
using CgmInfo.Utilities;

namespace CgmInfo.Commands.GraphicalPrimitives
{
    public class CircularArc3PointClose : Command
    {
        public CircularArc3PointClose(MetafilePoint start, MetafilePoint intermediate, MetafilePoint end, ArcClosureType closure)
            : base(4, 14)
        {
            Start = start;
            Intermediate = intermediate;
            End = end;
            Closure = closure;
        }

        public MetafilePoint Intermediate { get; private set; }
        public MetafilePoint Start { get; private set; }
        public MetafilePoint End { get; private set; }
        public ArcClosureType Closure { get; set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptGraphicalPrimitiveCircularArc3PointClose(this, parameter);
        }
    }
}
