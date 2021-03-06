﻿using CgmInfo.Traversal;
using CgmInfo.Utilities;

namespace CgmInfo.Commands.PictureDescriptor
{
    public class VdcExtent : Command
    {
        public VdcExtent(MetafilePoint firstCorner, MetafilePoint secondCorner)
            : base(2, 6)
        {
            FirstCorner = firstCorner;
            SecondCorner = secondCorner;
        }

        public MetafilePoint FirstCorner { get; private set; }
        public MetafilePoint SecondCorner { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptPictureDescriptorVdcExtent(this, parameter);
        }
    }
}
