using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Archive
{
    public class AlAarHeaderV2
    {
        // Magic and version omitted, handled in archive class
        public AlAarArchiveFlags Flags { get; set; }
        public ushort Count { get; set; }
        public uint LowId { get; set; }
        public uint HighId { get; set; }

        public void Read(BinaryReader br)
        {
            Flags = (AlAarArchiveFlags)br.ReadByte();
            Count = br.ReadUInt16();
            LowId = br.ReadUInt32();
            HighId = br.ReadUInt32();
        }
    }
}
