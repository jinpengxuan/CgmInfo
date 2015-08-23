using CgmInfo.Traversal;

namespace CgmInfo.Commands
{
    public sealed class UnsupportedCommand : Command
    {
        // command 15/127 is considered reserved for extension in binary encoding [ISO/IEC 8632-3 5.4]
        // considering classes 10-15 are still unused/reserved, I doubt that particular extension is needed.
        // until then, we'll simply abuse it as marker for unsupported Text Encoding commands.
        private const int ReservedElementClass_Text = 15;
        private const int ReservedElementId_Text = 127;

        public UnsupportedCommand(int elementClass, int elementId)
            : base(elementClass, elementId)
        {
        }

        public UnsupportedCommand(string elementName, string rawParameters)
            : base(ReservedElementClass_Text, ReservedElementId_Text)
        {
            ElementName = elementName;
            RawParameters = rawParameters;
        }

        /// <summary>
        /// Returns whether this unsupported command is based on the Binary Encoding
        /// (which uses <see cref="Command.ElementClass"/> and <see cref="Command.ElementId"/>
        /// to identify the given command) or the Text Encoding (which uses plain-text
        /// <see cref="ElementName"/> and a list of <see cref="RawParameters"/>).
        /// </summary>
        public bool IsTextEncoding
        {
            get { return ElementClass == ReservedElementClass_Text && ElementId == ReservedElementId_Text; }
        }
        /// <summary>Element Name in Text Encoding</summary>
        /// <remarks>Only set when <see cref="IsTextEncoding"/> is <c>true</c>.</remarks>
        public string ElementName { get; private set; }
        /// <summary>Raw parameter string as read from the file in Text Encoding</summary>
        /// <remarks>Only set when <see cref="IsTextEncoding"/> is <c>true</c>.</remarks>
        public string RawParameters { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptUnsupportedCommand(this, parameter);
        }
    }
}
