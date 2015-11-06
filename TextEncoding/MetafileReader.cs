﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CgmInfo.Commands;
using CgmInfo.Commands.Enums;
using CgmInfo.Commands.MetafileDescriptor;
using CgmInfo.Utilities;
using BaseMetafileReader = CgmInfo.MetafileReader;

namespace CgmInfo.TextEncoding
{
    public class MetafileReader : BaseMetafileReader
    {
        private readonly Dictionary<string, Func<MetafileReader, Command>> _commandTable = new Dictionary<string, Func<MetafileReader, Command>>
        {
            // delimiter elements [ISO/IEC 8632-4 7.1]
            { "BEGMF", DelimiterElementReader.BeginMetafile },
            { "ENDMF", DelimiterElementReader.EndMetafile },
            { "BEGPIC", DelimiterElementReader.BeginPicture },
            { "BEGPICBODY", DelimiterElementReader.BeginPictureBody },
            { "ENDPIC", DelimiterElementReader.EndPicture },
            { "BEGSEG", DelimiterElementReader.BeginSegment },
            { "ENDSEG", DelimiterElementReader.EndSegment },
            { "BEGFIGURE", DelimiterElementReader.BeginFigure },
            { "ENDFIGURE", DelimiterElementReader.EndFigure },
            { "BEGPROTREGION", DelimiterElementReader.BeginProtectionRegion },
            { "ENDPROTREGION", DelimiterElementReader.EndProtectionRegion },
            { "BEGCOMPOLINE", DelimiterElementReader.BeginCompoundLine },
            { "ENDCOMPOLINE", DelimiterElementReader.EndCompoundLine },
            { "BEGCOMPTEXTPATH", DelimiterElementReader.BeginCompoundTextPath },
            { "ENDCOMPTEXTPATH", DelimiterElementReader.EndCompoundTextPath },
            { "BEGTILEARRAY", DelimiterElementReader.BeginTileArray },
            { "ENDTILEARRAY", DelimiterElementReader.EndTileArray },
            { "BEGAPS", DelimiterElementReader.BeginApplicationStructure },
            { "BEGAPSBODY", DelimiterElementReader.BeginApplicationStructureBody },
            { "ENDAPS", DelimiterElementReader.EndApplicationStructure },

            // metafile descriptor elements [ISO/IEC 8632-4 7.2]
            { "MFVERSION", MetafileDescriptorReader.MetafileVersion },
            { "MFDESC", MetafileDescriptorReader.MetafileDescription },
            { "VDCTYPE", ReadVdcType },
            { "INTEGERPREC", ReadIntegerPrecision },
            { "REALPREC", ReadRealPrecision },
            { "INDEXPREC", ReadIndexPrecision },
            { "COLRPREC", ReadColorPrecision },
            { "COLRINDEXPREC", ReadColorIndexPrecision },
            { "MAXCOLRINDEX", MetafileDescriptorReader.MaximumColorIndex },
            { "COLRVALUEEXT", MetafileDescriptorReader.ColorValueExtent },
            { "MFELEMLIST", MetafileDescriptorReader.MetafileElementsList },
            { "BEGMFDEFAULTS", BeginMetafileDefaultsReplacement },
            { "ENDMFDEFAULTS", EndMetafileDefaultsReplacement },
            { "FONTLIST", MetafileDescriptorReader.FontList },
            { "CHARSETLIST", MetafileDescriptorReader.CharacterSetList },
            { "CHARCODING", MetafileDescriptorReader.CharacterCodingAnnouncer },
            { "NAMEPREC", ReadNamePrecision },
            { "MAXVDCEXT", MetafileDescriptorReader.MaximumVdcExtent },
            { "COLRMODEL", MetafileDescriptorReader.ColorModelCommand },

            // picture descriptor elements [ISO/IEC 8632-4 7.3]
            { "SCALEMODE", PictureDescriptorReader.ScalingMode },
            { "COLRMODE", ReadColorSelectionMode },
            { "LINEWIDTHMODE", ReadLineWidthSpecificationMode },
            { "MARKERSIZEMODE", ReadMarkerSizeSpecificationMode },
            { "EDGEWIDTHMODE", ReadEdgeWidthSpecificationMode },
            { "VDCEXT", PictureDescriptorReader.VdcExtent },
            { "BACKCOLR", PictureDescriptorReader.BackgroundColor },
            { "DEVVP", PictureDescriptorReader.DeviceViewport },
            { "DEVVPMODE", ReadDeviceViewportSpecificationMode },
            { "INTSTYLEMODE", ReadInteriorStyleSpecificationMode },
            { "LINEEDGETYPEDEF", PictureDescriptorReader.LineAndEdgeTypeDefinition },
            { "HATCHSTYLEDEF", PictureDescriptorReader.HatchStyleDefinition },
            { "GEOPATDEF", PictureDescriptorReader.GeometricPatternDefinition },

            // control elements [ISO/IEC 8632-4 7.4]
            { "VDCINTEGERPREC", ReadVdcIntegerPrecision },
            { "VDCREALPREC", ReadVdcRealPrecision },
            { "AUXCOLR", ControlElementReader.AuxiliaryColor },
            { "TRANSPARENCY", ControlElementReader.Transparency },
            { "CLIPRECT", ControlElementReader.ClipRectangle },
            { "CLIP", ControlElementReader.ClipIndicator },
            { "LINECLIPMODE", ControlElementReader.LineClippingMode },
            { "MARKERCLIPMODE", ControlElementReader.MarkerClippingMode },
            { "EDGECLIPMODE", ControlElementReader.EdgeClippingMode },
            { "NEWREGION", ControlElementReader.NewRegion },
            { "SAVEPRIMCONT", ControlElementReader.SavePrimitiveContext },
            { "RESPRIMCONT", ControlElementReader.RestorePrimitiveContext },
            { "PROTREGION", ControlElementReader.ProtectionRegionIndicator },
            { "GENTEXTPATHMODE", ControlElementReader.GeneralizedTextPathMode },
            { "MITRELIMIT", ControlElementReader.MiterLimit },

            // graphical primitive elements [ISO/IEC 8632-4 7.5]
            { "LINE", GraphicalPrimitiveReader.Polyline },
            { "INCRLINE", GraphicalPrimitiveReader.IncrementalPolyline },
            { "TEXT", GraphicalPrimitiveReader.Text },
            { "RESTRTEXT", GraphicalPrimitiveReader.RestrictedText },
            { "APNDTEXT", GraphicalPrimitiveReader.AppendText },
            { "POLYGON", GraphicalPrimitiveReader.Polygon },
            { "INCRPOLYGON", GraphicalPrimitiveReader.IncrementalPolygon },
            { "RECT", GraphicalPrimitiveReader.Rectangle },
            { "CIRCLE", GraphicalPrimitiveReader.Circle },
            { "ARCCTR", GraphicalPrimitiveReader.CircularArcCenter },
            { "ELLIPSE", GraphicalPrimitiveReader.Ellipse },
            { "ELLIPARC", GraphicalPrimitiveReader.EllipticalArc },

            // attribute elements [ISO/IEC 8632-4 7.6]
            { "LINEINDEX", AttributeReader.LineBundleIndex },
            { "LINETYPE", AttributeReader.LineType },
            { "LINEWIDTH", AttributeReader.LineWidth },
            { "LINECOLR", AttributeReader.LineColor },
            { "MARKERINDEX", AttributeReader.MarkerBundleIndex },
            { "MARKERTYPE", AttributeReader.MarkerType },
            { "MARKERSIZE", AttributeReader.MarkerSize },
            { "MARKERCOLR", AttributeReader.MarkerColor },
            { "TEXTINDEX", AttributeReader.TextBundleIndex },
            { "TEXTFONTINDEX", AttributeReader.TextFontIndex },
            { "TEXTPREC", AttributeReader.TextPrecision },
            { "CHAREXPAN", AttributeReader.CharacterExpansionFactor },
            { "CHARSPACE", AttributeReader.CharacterSpacing },
            { "TEXTCOLR", AttributeReader.TextColor },
            { "CHARHEIGHT", AttributeReader.CharacterHeight },
            { "CHARORI", AttributeReader.CharacterOrientation },
            { "TEXTPATH", AttributeReader.TextPath },
            { "TEXTALIGN", AttributeReader.TextAlignment },
            { "CHARSETINDEX", AttributeReader.CharacterSetIndex },
            { "ALTCHARSETINDEX", AttributeReader.AlternateCharacterSetIndex },
            { "FILLINDEX", AttributeReader.FillBundleIndex },
            { "INTSTYLE", AttributeReader.InteriorStyle },
            { "FILLCOLR", AttributeReader.FillColor },
            { "HATCHINDEX", AttributeReader.HatchIndex },
            { "PATINDEX", AttributeReader.PatternIndex },
            { "EDGEINDEX", AttributeReader.EdgeBundleIndex },
            { "EDGETYPE", AttributeReader.EdgeType },
            { "EDGEWIDTH", AttributeReader.EdgeWidth },
            { "EDGECOLR", AttributeReader.EdgeColor },
            { "EDGEVIS", AttributeReader.EdgeVisibility },
            { "FILLREFPT", AttributeReader.FillReferencePoint },
            { "PATTABLE", AttributeReader.PatternTable },

            // application structure descriptor elements [ISO/IEC 8632-4 7.10]
            { "APSATTR", ApplicationStructureDescriptorReader.ApplicationStructureAttribute },
        };

        public MetafileReader(string fileName)
            : base(fileName, false)
        {
        }

        private long _commandPosition;
        private int _currentTokenIndex;
        private List<string> _currentTokens;

        protected override Command ReadCommand(Stream stream)
        {
            // remember the current position for error feedback.
            // this always signifies the beginning of the command; not the token.
            _commandPosition = stream.Position;
            // read all tokens until end of either command or file
            TokenState state;
            var tokens = new List<string>();
            do
            {
                string token;
                state = ReadToken(stream, out token);
                tokens.Add(token);
            } while (state == TokenState.EndOfToken);

            if (state == TokenState.EndOfFile)
                return null;

            try
            {
                _currentTokens = tokens;
                _currentTokenIndex = 0;

                string elementName = ReadToken();
                Func<MetafileReader, Command> commandHandler;
                if (!_commandTable.TryGetValue(elementName.ToUpperInvariant(), out commandHandler))
                    commandHandler = r => UnsupportedCommand(elementName);

                return commandHandler(this);
            }
            finally
            {
                _currentTokens = null;
            }
        }

        private Command UnsupportedCommand(string elementName)
        {
            return new UnsupportedCommand(elementName, string.Join(" ", _currentTokens.Skip(1)));
        }

        private static Command BeginMetafileDefaultsReplacement(MetafileReader reader)
        {
            var commands = new List<Command>();
            while (true)
            {
                var command = reader.Read();
                if (command == null)
                    break;
                commands.Add(command);
            }
            return new MetafileDefaultsReplacement(commands.ToArray());
        }
        private static Command EndMetafileDefaultsReplacement(MetafileReader reader)
        {
            // null signals end-of-file, but since ENDMFDEFAULTS must have a corresponding BEDMFDEFAULTS,
            // we'll just use it to end the loop in there in the same manner.
            return null;
        }

        private static Command ReadVdcType(MetafileReader reader)
        {
            var vdcType = MetafileDescriptorReader.VdcType(reader);
            reader.Descriptor.VdcType = vdcType.Specification;
            return vdcType;
        }
        private static Command ReadIntegerPrecision(MetafileReader reader)
        {
            var integerPrecision = MetafileDescriptorReader.IntegerPrecision(reader);
            reader.Descriptor.IntegerPrecision = integerPrecision.Precision;
            return integerPrecision;
        }
        private static Command ReadRealPrecision(MetafileReader reader)
        {
            var realPrecision = MetafileDescriptorReader.RealPrecision(reader);
            reader.Descriptor.RealPrecision = realPrecision.Specification;
            return realPrecision;
        }
        private static Command ReadIndexPrecision(MetafileReader reader)
        {
            var indexPrecision = MetafileDescriptorReader.IndexPrecision(reader);
            reader.Descriptor.IndexPrecision = indexPrecision.Precision;
            return indexPrecision;
        }
        private static Command ReadColorPrecision(MetafileReader reader)
        {
            var colorPrecision = MetafileDescriptorReader.ColorPrecision(reader);
            reader.Descriptor.ColorPrecision = colorPrecision.Precision;
            return colorPrecision;
        }
        private static Command ReadColorIndexPrecision(MetafileReader reader)
        {
            var colorIndexPrecision = MetafileDescriptorReader.ColorIndexPrecision(reader);
            reader.Descriptor.ColorIndexPrecision = colorIndexPrecision.Precision;
            return colorIndexPrecision;
        }
        private static Command ReadNamePrecision(MetafileReader reader)
        {
            var namePrecision = MetafileDescriptorReader.NamePrecision(reader);
            reader.Descriptor.NamePrecision = namePrecision.Precision;
            return namePrecision;
        }
        private static Command ReadColorSelectionMode(MetafileReader reader)
        {
            var colorSelectionMode = PictureDescriptorReader.ColorSelectionMode(reader);
            reader.Descriptor.ColorSelectionMode = colorSelectionMode.ColorMode;
            return colorSelectionMode;
        }
        private static Command ReadLineWidthSpecificationMode(MetafileReader reader)
        {
            var lineWidthSpecificationMode = PictureDescriptorReader.LineWidthSpecificationMode(reader);
            reader.Descriptor.LineWidthSpecificationMode = lineWidthSpecificationMode.WidthSpecificationMode;
            return lineWidthSpecificationMode;
        }
        private static Command ReadMarkerSizeSpecificationMode(MetafileReader reader)
        {
            var markerSizeSpecificationMode = PictureDescriptorReader.MarkerSizeSpecificationMode(reader);
            reader.Descriptor.MarkerSizeSpecificationMode = markerSizeSpecificationMode.WidthSpecificationMode;
            return markerSizeSpecificationMode;
        }
        private static Command ReadEdgeWidthSpecificationMode(MetafileReader reader)
        {
            var edgeWidthSpecificationMode = PictureDescriptorReader.EdgeWidthSpecificationMode(reader);
            reader.Descriptor.EdgeWidthSpecificationMode = edgeWidthSpecificationMode.WidthSpecificationMode;
            return edgeWidthSpecificationMode;
        }
        private static Command ReadDeviceViewportSpecificationMode(MetafileReader reader)
        {
            var deviceViewportSpecificationMode = PictureDescriptorReader.DeviceViewportSpecificationMode(reader);
            reader.Descriptor.DeviceViewportSpecificationMode = deviceViewportSpecificationMode.SpecificationMode;
            return deviceViewportSpecificationMode;
        }
        private static Command ReadInteriorStyleSpecificationMode(MetafileReader reader)
        {
            var interiorStyleSpecificationMode = PictureDescriptorReader.InteriorStyleSpecificationMode(reader);
            reader.Descriptor.InteriorStyleSpecificationMode = interiorStyleSpecificationMode.WidthSpecificationMode;
            return interiorStyleSpecificationMode;
        }
        private static Command ReadVdcIntegerPrecision(MetafileReader reader)
        {
            var vdcIntegerPrecision = ControlElementReader.VdcIntegerPrecision(reader);
            reader.Descriptor.VdcIntegerPrecision = vdcIntegerPrecision.Precision;
            return vdcIntegerPrecision;
        }
        private static Command ReadVdcRealPrecision(MetafileReader reader)
        {
            var vdcRealPrecision = ControlElementReader.VdcRealPrecision(reader);
            reader.Descriptor.VdcRealPrecision = vdcRealPrecision.Specification;
            return vdcRealPrecision;
        }

        internal string ReadString()
        {
            return ReadToken();
        }

        private string ReadToken()
        {
            if (_currentTokens == null || _currentTokenIndex >= _currentTokens.Count)
                return null;
            return _currentTokens[_currentTokenIndex++];
        }

        internal string ReadEnum()
        {
            return ReadToken();
        }
        internal int ReadIndex()
        {
            return ReadInteger();
        }
        internal int ReadName()
        {
            return ReadInteger();
        }
        private static readonly Regex DecimalInteger = new Regex(@"^(?<sign>[+\-])?(?<digits>[0-9]+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex BasedInteger = new Regex(@"^(?<sign>[+\-])?(?<radix>(?:[2-9]|1[0-6]))#(?<digits>[0-9A-F]+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
        private const string ExtendedDigits = "0123456789ABCDEF";
        internal int ReadInteger()
        {
            string number = ReadToken();
            var match = DecimalInteger.Match(number);
            if (match.Success)
            {
                int num;
                if (!int.TryParse(match.Groups["digits"].Value, out num))
                    throw new FormatException(string.Format("Invalid Decimal Integer digits '{0}' at command position {1}", number, _commandPosition));
                if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                    num = -num;
                return num;
            }

            match = BasedInteger.Match(number);
            if (match.Success)
            {
                int radix;
                if (!int.TryParse(match.Groups["radix"].Value, out radix))
                    throw new FormatException(string.Format("Invalid Based Integer radix '{0}' at command position {1}", number, _commandPosition));
                int num = 0;
                string digits = match.Groups["digits"].Value.ToUpperInvariant();
                for (int i = 0; i < digits.Length; i++)
                {
                    int charValue = ExtendedDigits.IndexOf(digits[i]);
                    if (charValue < 0 || charValue >= radix)
                        throw new ArgumentOutOfRangeException("BasedInteger", digits[charValue],
                            string.Format("Invalid Based Integer digits '{0}' at command position {1}", number, _commandPosition));

                    num = num * radix + charValue;
                }
                if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                    num = -num;
                return num;
            }

            throw new FormatException(string.Format("Unsupported Integer Format '{0}' at command position {1}", number, _commandPosition));
        }
        // according to spec, explicit point must have either 1 digit integer or 1 digit fraction. lets hope we can get away with that...[ISO/IEC 8632-4 6.3.2]
        private static readonly Regex ExplicitPointNumber = new Regex(@"^(?<sign>[+\-])?(?<integer>[0-9]*)\.(?<fraction>[0-9]*)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
        private static readonly Regex ScaledRealNumber = new Regex(@"^(?<sign>[+\-])?(?<integer>[0-9]*)(?:\.(?<fraction>[0-9]*))?[Ee](?<exponent>[0-9]+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
        internal double ReadReal()
        {
            string number = ReadToken();

            // all 3 formats can be parsed by double.Parse, so simply throw if the number itself doesn't match either.
            if (!ExplicitPointNumber.IsMatch(number) && !ScaledRealNumber.IsMatch(number) && !DecimalInteger.IsMatch(number))
                throw new FormatException(string.Format("Invalid Real number '{0}' at command position {1}", number, _commandPosition));

            return double.Parse(number, CultureInfo.GetCultureInfo("en"));
        }
        internal double ReadSizeSpecification(WidthSpecificationModeType widthSpecificationMode)
        {
            // When the value is 'absolute', then an associated parameter of type SS
            // resolves to the basic data type VDC. Otherwise, associated
            // SS parameters resolve to the basic data type R. [ISO/IEC 8632-1 7.1, Table 11]
            if (widthSpecificationMode == WidthSpecificationModeType.Absolute)
                return ReadVdc();
            else
                return ReadReal();
        }
        internal double ReadVdc()
        {
            // a VDC is either an int or a double; depending on what VDC TYPE said [ISO/IEC 8632-4 6.3.5]
            if (Descriptor.VdcType == VdcTypeSpecification.Integer)
            {
                return ReadInteger();
            }
            else if (Descriptor.VdcType == VdcTypeSpecification.Real)
            {
                return ReadReal();
            }

            throw new NotSupportedException("The current VDC TYPE is not supported");
        }
        internal PointF ReadPoint()
        {
            double x = ReadVdc();
            double y = ReadVdc();
            return new PointF((float)x, (float)y);
        }
        internal double ReadViewportCoordinate()
        {
            // a Viewport Coordinate (VC) is either an int or a double; depending on what DEVICE VIEWPORT SPECIFICATION MODE said [ISO/IEC 8632-4 6.3.5]
            if (Descriptor.DeviceViewportSpecificationMode == DeviceViewportSpecificationModeType.MillimetersWithScaleFactor ||
                Descriptor.DeviceViewportSpecificationMode == DeviceViewportSpecificationModeType.PhysicalDeviceCoordinates)
            {
                return ReadInteger();
            }
            else if (Descriptor.DeviceViewportSpecificationMode == DeviceViewportSpecificationModeType.FractionOfDrawingSurface)
            {
                return ReadReal();
            }

            throw new NotSupportedException("The current DEVICE VIEWPORT SPECIFICATION MODE is not supported");
        }
        internal PointF ReadViewportPoint()
        {
            double x = ReadViewportCoordinate();
            double y = ReadViewportCoordinate();
            return new PointF((float)x, (float)y);
        }
        internal MetafileColor ReadColor()
        {
            if (Descriptor.ColorSelectionMode == ColorModeType.Direct)
                return ReadDirectColor();
            else
                return ReadIndexedColor();
        }
        internal MetafileColor ReadIndexedColor()
        {
            return new MetafileColorIndexed(ReadIndex());
        }
        internal MetafileColor ReadDirectColor()
        {
            if (Descriptor.ColorModel == ColorModel.RGB)
            {
                int r = ReadColorValue();
                int g = ReadColorValue();
                int b = ReadColorValue();
                return new MetafileColorRGB(r, g, b);
            }
            else if (Descriptor.ColorModel == ColorModel.CMYK)
            {
                int c = ReadColorValue();
                int m = ReadColorValue();
                int y = ReadColorValue();
                int k = ReadColorValue();
                return new MetafileColorCMYK(c, m, y, k);
            }
            else
            {
                double first = ReadReal();
                double second = ReadReal();
                double third = ReadReal();
                return new MetafileColorCIE(Descriptor.ColorModel, first, second, third);
            }
        }
        internal int ReadColorValue()
        {
            // FIXME: color component in CIELAB/CIELUV/RGB-related is reals, not ints
            return ReadInteger();
        }

        internal bool HasMoreData()
        {
            return HasMoreData(1);
        }
        internal bool HasMoreData(int minimumLeft)
        {
            return _currentTokens == null || _currentTokenIndex + minimumLeft <= _currentTokens.Count;
        }
        private static TokenState ReadToken(Stream stream, out string token)
        {
            var sb = new StringBuilder();
            try
            {
                while (stream.Position < stream.Length)
                {
                    char c = (char)stream.ReadByte();
                    switch (c)
                    {
                        // null characters; skip them [ISO/IEC 8632-4 6.1]
                        case '_':
                        case '$':
                            break;

                        // element seperator characters; those end the current element [ISO/IEC 8632-4 6.2.1]
                        case ';':
                        case '/':
                            return TokenState.EndOfElement;

                        // whitespace; skip them unless there is content (since it also counts as a soft seperator) [ISO/IEC 8632-4 6.2.2]
                        case ' ':
                        case '\r':
                        case '\n':
                        case '\t':
                        // point values might be enclosed in parentheses for readability [ISO/IEC 8632-4 6.3.5]
                        // TODO: spec says there must be exactly two numbers inside the parentheses, or the file is non-conforming.
                        //       this isn't validated at this point, and would require rewriting ReadPoint (and possibly others).
                        // instead, we treat them as soft separators (ie.: return content if there is any, but keep parsing if there is not)
                        case '(':
                        case ')':
                            if (sb.Length > 0)
                                return TokenState.EndOfToken;
                            break;

                        // comma counts as a hard separator [ISO/IEC 8632-4 6.2.2]
                        case ',':
                            return TokenState.EndOfToken;

                        // strings; fully read and return them as they are [ISO/IEC 8632-4 6.3.3]
                        case '\'':
                        case '"':
                            char stringDelimiter = c;
                            do
                            {
                                c = (char)stream.ReadByte();

                                // in case the delimiter appears:
                                // either end of string, or double the delimiter to include a literal one
                                if (c == stringDelimiter)
                                {
                                    if (stream.Position >= stream.Length)
                                        return TokenState.EndOfFile;

                                    char nextChar = (char)stream.ReadByte();
                                    if (nextChar == stringDelimiter)
                                    {
                                        // literal delimiter; append it once, then reset the character to keep the loop going
                                        sb.Append(c);
                                        c = '\x00';
                                    }
                                    else
                                    {
                                        // end of string; reset back by the one character read ahead
                                        stream.Seek(-1, SeekOrigin.Current);
                                        break;
                                    }
                                }
                                else
                                {
                                    // any other character: simply append to the token string
                                    sb.Append(c);
                                }

                                if (stream.Position >= stream.Length)
                                    return TokenState.EndOfFile;
                            } while (c != stringDelimiter);

                            // end of string might also mean end of element;
                            // we need to do another read, or we'd end up with an empty string at the next read
                            // simply break for another loop; and pray we get either a "/" or ";" character next.
                            break;

                        // comment; skip them completely [ISO/IEC 8632-4 6.2.3]
                        case '%':
                            do
                            {
                                c = (char)stream.ReadByte();
                                if (stream.Position >= stream.Length)
                                    return TokenState.EndOfFile;
                            } while (c != '%');
                            // Comments may be included any place that a separator may be used, and are equivalent to a <SOFTSEP>; they
                            // may be replaced by a SPACE character in parsing, without affecting the meaning of the metafile.
                            return TokenState.EndOfToken;

                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }
            finally
            {
                token = sb.ToString();
            }
            return TokenState.EndOfFile;
        }

        private enum TokenState
        {
            // obviously EOF
            EndOfFile,
            // reached an element seperator (either ";" or "/")
            EndOfElement,
            // reached something that delimits tokens from each other; but doesn't end the element
            EndOfToken,
        }
    }
}
