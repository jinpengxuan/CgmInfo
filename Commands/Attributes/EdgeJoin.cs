using System.Collections.Generic;
using System.Collections.ObjectModel;
using CgmInfo.Traversal;

namespace CgmInfo.Commands.Attributes
{
    public class EdgeJoin : Command
    {
        public EdgeJoin(int index)
            : base(5, 45)
        {
            Index = index;
            Name = GetName(index);
        }

        public int Index { get; private set; }
        public string Name { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptAttributeEdgeJoin(this, parameter);
        }

        private static readonly ReadOnlyDictionary<int, string> _knownEdgeJoinIndicators = new ReadOnlyDictionary<int, string>(new Dictionary<int, string>
        {
            // edge join indicators originally part of ISO/IEC 8632:1999
            { 1, "Unspecified" },
            { 2, "Mitre" },
            { 3, "Round" },
            { 4, "Bevel" },
        });
        public static IReadOnlyDictionary<int, string> KnownEdgeJoinIndicators
        {
            get { return _knownEdgeJoinIndicators; }
        }
        public static string GetName(int index)
        {
            string name;
            if (KnownEdgeJoinIndicators.TryGetValue(index, out name))
                return name;

            return "Reserved";
        }
    }
}