using CgmInfo.Traversal;
using CgmInfo.Utilities;

namespace CgmInfo.Commands.GraphicalPrimitives
{
    public class NonUniformRationalBSpline : Command
    {
        public NonUniformRationalBSpline(int splineOrder, MetafilePoint[] controlPoints, double[] knots, double start, double end, double[] weights)
            : base(4, 25)
        {
            SplineOrder = splineOrder;
            ControlPoints = controlPoints;
            Knots = knots;
            Start = start;
            End = end;
            Weights = weights;
        }

        public int SplineOrder { get; private set; }
        public MetafilePoint[] ControlPoints { get; private set; }
        public double[] Knots { get; private set; }
        public double Start { get; private set; }
        public double End { get; private set; }
        public double[] Weights { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptGraphicalPrimitiveNonUniformRationalBSpline(this, parameter);
        }
    }
}
