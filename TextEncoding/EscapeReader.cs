using CgmInfo.Commands;
using CgmInfo.Commands.Enums;
using CgmInfo.Commands.Escape;

namespace CgmInfo.TextEncoding
{
    internal static class EscapeReader
    {
        public static EscapeCommand Escape(MetafileReader reader)
        {
            int identifier = reader.ReadInteger();

            string dataRecordString = reader.ReadString();
            // attempt to parse the data record as structured record, in case it is a known one
            // otherwise it is probably application specific and cannot be assumed to be a structured record
            StructuredDataRecord dataRecord;
            if (EscapeCommand.KnownEscapeTypes.ContainsKey(identifier))
                dataRecord = ApplicationStructureDescriptorReader.ParseStructuredDataRecord(dataRecordString);
            else
                dataRecord = new StructuredDataRecord(new[]
                {
                    new StructuredDataElement(DataTypeIndex.String, new object[] { dataRecordString }),
                });
            return new EscapeCommand(identifier, dataRecord);
        }
    }
}
