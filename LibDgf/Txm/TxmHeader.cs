using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Txm
{
    public class TxmHeader
    {
        public TxmPixelFormat ImageSourcePixelFormat { get; set; }
        public TxmPixelFormat ImageVideoPixelFormat { get; set; }
        public short ImageWidth { get; set; }
        public short ImageHeight { get; set; }
        public short ImageBufferBase { get; set; }
        public TxmPixelFormat ClutPixelFormat { get; set; }
        public byte Misc { get; set; } // 0x0f = level, 0x70 = count, 0x80 = fast count
        public short ClutWidth { get; set; }
        public short ClutHeight { get; set; }
        public short ClutBufferBase { get; set; }

        public void Read(BinaryReader br)
        {
            ImageSourcePixelFormat = (TxmPixelFormat)br.ReadByte();
            ImageVideoPixelFormat = (TxmPixelFormat)br.ReadByte();
            ImageWidth = br.ReadInt16();
            ImageHeight = br.ReadInt16();
            ImageBufferBase = br.ReadInt16();
            ClutPixelFormat = (TxmPixelFormat)br.ReadByte();
            Misc = br.ReadByte();
            ClutWidth = br.ReadInt16();
            ClutHeight = br.ReadInt16();
            ClutBufferBase = br.ReadInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)ImageSourcePixelFormat);
            bw.Write((byte)ImageVideoPixelFormat);
            bw.Write(ImageWidth);
            bw.Write(ImageHeight);
            bw.Write(ImageBufferBase);
            bw.Write((byte)ClutPixelFormat);
            bw.Write(Misc);
            bw.Write(ClutWidth);
            bw.Write(ClutHeight);
            bw.Write(ClutBufferBase);
        }

        public int GetImageByteSize()
        {
            return GetImageMemSize(ImageSourcePixelFormat, ImageWidth, ImageHeight);
        }

        public int GetClutByteSize()
        {
            return GetImageMemSize(ClutPixelFormat, ClutWidth, ClutHeight);
        }

        int GetImageMemSize(TxmPixelFormat format, int width, int height)
        {
            switch (format)
            {
                case TxmPixelFormat.PSMT4:
                    return width * height / 2;
                case TxmPixelFormat.PSMT8:
                    return width * height;
                case TxmPixelFormat.PSMCT32:
                    return width * height * 4;
                default:
                    return 0;
            }
        }

        public override string ToString()
        {
            return $"{ImageSourcePixelFormat} {ImageVideoPixelFormat} {ImageWidth}x{ImageHeight} {ImageBufferBase:x4} " + 
                $"{ClutPixelFormat} {Misc:x2} {ClutWidth}x{ClutHeight} {ClutBufferBase:x4}";
        }
    }
}
