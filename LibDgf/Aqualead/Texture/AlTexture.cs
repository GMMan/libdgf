using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Texture
{
    public class AlTexture
    {
        public AlTextureFlags Flags { get; set; }
        public List<AlTextureEntry> ChildTextures { get; set; } = new List<AlTextureEntry>();
        public short Width { get; set; }
        public short Height { get; set; }
        public uint IndirectReference { get; set; }
        public byte[] ImageData { get; set; }

        public void Read(BinaryReader br)
        {
            var baseOffset = br.BaseStream.Position;

            if (new string(br.ReadChars(4)) != "ALTX")
                throw new InvalidDataException("Not an Aqualead texture.");

            bool isMultitexture = br.ReadBoolean();
            Flags = (AlTextureFlags)br.ReadByte();
            ushort numTextures = br.ReadUInt16();
            uint imgOffset = br.ReadUInt32();
            ushort[] entryOffsets = new ushort[numTextures];
            for (int i = 0; i < numTextures; ++i)
            {
                entryOffsets[i] = br.ReadUInt16();
            }

            ChildTextures.Clear();
            uint entryOffset = 0;
            for (int i = 0; i < numTextures; ++i)
            {
                entryOffset += entryOffsets[i];
                br.BaseStream.Seek(baseOffset + entryOffset, SeekOrigin.Begin);
                AlTextureEntry entry = new AlTextureEntry();
                entry.Read(br, isMultitexture);
                ChildTextures.Add(entry);
            }

            if ((Flags & AlTextureFlags.IsSpecial) != 0)
            {
                if ((Flags & AlTextureFlags.IsIndirect) != 0)
                {
                    IndirectReference = br.ReadUInt32();
                    return;
                }
                else if ((Flags & AlTextureFlags.IsOverrideDimensions) != 0)
                {
                    Width = br.ReadInt16();
                    Height = br.ReadInt16();
                }
            }

            br.BaseStream.Seek(baseOffset + imgOffset, SeekOrigin.Begin);
            ImageData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
        }
    }
}
