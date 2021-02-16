using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Image
{
    public class AlImage
    {
        string pixelFormat;

        public AlImageMipmapType MipmapType { get; set; }
        public AlImageFlags Flags { get; set; }
        public string PixelFormat
        {
            get
            {
                return pixelFormat;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length > 4) throw new ArgumentException("String length too long.", nameof(value));
                pixelFormat = value;
            }
        }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public byte[] PlatformWork { get; set; }
        public uint[] PaletteColors { get; set; }
        public List<byte[]> Mipmaps { get; set; }

        public void Read(BinaryReader br)
        {
            var baseOffset = br.BaseStream.Position;
            if (new string(br.ReadChars(4)) != "ALIG")
                throw new InvalidDataException("Not an AquaLead image.");
            MipmapType = (AlImageMipmapType)br.ReadByte();
            Flags = (AlImageFlags)br.ReadByte();
            int numPaletteColors = br.ReadUInt16();
            if ((Flags & AlImageFlags.PaletteAdd64K) != 0) numPaletteColors += 0x10000;
            if ((Flags & AlImageFlags.PaletteAdd128K) != 0) numPaletteColors += 0x20000;
            PixelFormat = new string(br.ReadChars(4));
            br.ReadUInt32();
            if (MipmapType != AlImageMipmapType.Platform)
            {
                Width = br.ReadUInt32();
                Height = br.ReadUInt32();
            }
            else
            {
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
            }

            int numMipmaps = MipmapType == AlImageMipmapType.NoMipmap ? 1 : br.ReadUInt16();
            ushort paletteOffset = br.ReadUInt16();
            ushort platformWorkLength = MipmapType == AlImageMipmapType.Platform ? br.ReadUInt16() : (ushort)0;

            uint[] mipmapOffsets = new uint[numMipmaps + 1];
            if (MipmapType == AlImageMipmapType.NoMipmap)
            {
                mipmapOffsets[0] = br.ReadUInt16();
            }
            else
            {
                for (int i = 0; i < numMipmaps; ++i)
                {
                    mipmapOffsets[i] = br.ReadUInt32();
                }
            }
            mipmapOffsets[numMipmaps] = (uint)(br.BaseStream.Length - baseOffset);

            PaletteColors = new uint[numPaletteColors];
            br.BaseStream.Seek(baseOffset + paletteOffset, SeekOrigin.Begin);
            for (int i = 0; i < PaletteColors.Length; ++i)
            {
                PaletteColors[i] = br.ReadUInt32();
            }

            if (MipmapType == AlImageMipmapType.Platform)
            {
                br.BaseStream.Seek(baseOffset + 0x20, SeekOrigin.Begin);
                PlatformWork = br.ReadBytes(platformWorkLength);
            }

            Mipmaps = new List<byte[]>();
            for (int i = 0; i < numMipmaps; ++i)
            {
                br.BaseStream.Seek(baseOffset + mipmapOffsets[i], SeekOrigin.Begin);
                Mipmaps.Add(br.ReadBytes((int)(mipmapOffsets[i + 1] - mipmapOffsets[i])));
            }
        }
    }
}
