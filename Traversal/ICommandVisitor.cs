﻿using CgmInfo.Commands;
using CgmInfo.Commands.Delimiter;
using CgmInfo.Commands.MetafileDescriptor;

namespace CgmInfo.Traversal
{
    public interface ICommandVisitor<T>
    {
        void AcceptUnsupportedCommand(UnsupportedCommand unsupportedCommand, T parameter);

        // delimiters
        void AcceptDelimiterBeginMetafile(BeginMetafile beginMetafile, T parameter);

        // metafile descriptor
        void AcceptMetafileDescriptorMetafileVersion(MetafileVersion metafileVersion, T parameter);
        void AcceptMetafileDescriptorMetafileDescription(MetafileDescription metafileDescription, T parameter);
        void AcceptMetafileDescriptorVdcType(VdcType vdcType, T parameter);
        void AcceptMetafileDescriptorIntegerPrecision(IntegerPrecision integerPrecision, T parameter);
        void AcceptMetafileDescriptorRealPrecision(RealPrecision realPrecision, T parameter);
        void AcceptMetafileDescriptorIndexPrecision(IndexPrecision indexPrecision, T parameter);
        void AcceptMetafileDescriptorColorPrecision(ColorPrecision colorPrecision, T parameter);
        void AcceptMetafileDescriptorColorIndexPrecision(ColorIndexPrecision colorIndexPrecision,T parameter);
        void AcceptMetafileDescriptorMaximumColorIndex(MaximumColorIndex maximumColorIndex, T parameter);
        void AcceptMetafileDescriptorColorValueExtent(ColorValueExtent colorValueExtent, T parameter);
        void AcceptMetafileDescriptorFontList(FontList fontList, T parameter);
        void AcceptMetafileDescriptorNamePrecision(NamePrecision namePrecision, T parameter);
        void AcceptMetafileDescriptorMaximumVdcExtent(MaximumVdcExtent maximumVdcExtent, T parameter);
        void AcceptMetafileDescriptorColorModel(ColorModelCommand colorModel, T parameter);
    }
}
