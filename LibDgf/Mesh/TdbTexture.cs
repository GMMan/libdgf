using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Mesh
{
    public class TdbTexture
    {
        public ushort DatIndex { get; set; }
        public ushort ImageBufferBase { get; set; }
        public ushort ClutBufferBase { get; set; }

        public void Read(BinaryReader br)
        {
            DatIndex = br.ReadUInt16();
            ImageBufferBase = br.ReadUInt16();
            ClutBufferBase = br.ReadUInt16();
        }
    }
}
