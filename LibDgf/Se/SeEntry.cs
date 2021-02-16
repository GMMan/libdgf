using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Se
{
    public class SeEntry
    {
        public int DataOffset { get; set; }
        public int DataLength { get; set; }
        public uint SeLength { get; set; }
        public uint Unknown { get; set; }

        public void Read(BinaryReader br)
        {
            DataOffset = br.ReadInt32();
            DataLength = br.ReadInt32();
            SeLength = br.ReadUInt32();
            Unknown = br.ReadUInt32();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(DataOffset);
            bw.Write(DataLength);
            bw.Write(SeLength);
            bw.Write(Unknown);
        }
    }
}
