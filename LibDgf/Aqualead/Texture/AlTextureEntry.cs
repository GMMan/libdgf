using GMWare.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Texture
{
    public class AlTextureEntry
    {
        public uint Id { get; set; }
        public AlTextureEntryFlags Flags { get; set; }
        public string Name { get; set; }
        public List<AlXYWH> Bounds { get; set; } = new List<AlXYWH>();
        public AlPoint CenterPoint { get; set; }

        public void Read(BinaryReader br, bool isMultiTexture)
        {
            var baseOffset = br.BaseStream.Position;
            Id = br.ReadUInt32();
            ushort mipsCount = br.ReadUInt16();
            Flags = (AlTextureEntryFlags)br.ReadByte();
            br.ReadByte(); // Alignment
            if (!isMultiTexture) return;

            for (int i = 0; i < mipsCount; ++i)
            {
                Bounds.Add(new AlXYWH
                {
                    X = br.ReadInt16(),
                    Y = br.ReadInt16(),
                    W = br.ReadUInt16(),
                    H = br.ReadUInt16()
                });
            }

            if ((Flags & AlTextureEntryFlags.IsHasCenterPoint) != 0)
            {
                CenterPoint = new AlPoint
                {
                    X = br.ReadInt16(),
                    Y = br.ReadInt16()
                };
            }

            if ((Flags & AlTextureEntryFlags.IsHasName) != 0)
            {
                br.BaseStream.Seek(baseOffset - 0x20, SeekOrigin.Begin);
                Name = StringReadingHelper.ReadNullTerminatedStringFromFixedSizeBlock(br, 0x20, Encoding.UTF8);
            }
        }
    }
}
