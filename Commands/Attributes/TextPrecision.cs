using CgmInfo.Commands.Enums;
using CgmInfo.Traversal;

namespace CgmInfo.Commands.Attributes
{
    public class TextPrecision : Command
    {
        public TextPrecision(TextPrecisionType precision)
            : base(5, 11)
        {
            Precision = precision;
        }

        public TextPrecisionType Precision { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptAttributeTextPrecision(this, parameter);
        }
    }
}
