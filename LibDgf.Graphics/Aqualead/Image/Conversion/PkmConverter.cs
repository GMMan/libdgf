using LibDgf.Aqualead.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Image.Conversion
{
    public class PkmConverter : IAlImageConverter
    {
        public string FileExtension => ".pkm";

        public void ConvertFromAl(AlImage image, Stream destStream)
        {
            List<byte[]> mips;
            if (image.PixelFormat == "ETC1")
            {
                mips = image.Mipmaps;
            }
            else if (image.PixelFormat == "EC1A")
            {
                // Give first half of the mips
                mips = new List<byte[]>();
                byte[] newMip = new byte[image.Mipmaps[0].Length / 2];
                Buffer.BlockCopy(image.Mipmaps[0], 0, newMip, 0, newMip.Length);
                mips.Add(newMip);
            }
            else
            {
                throw new ArgumentException("Pixel format not supported.", nameof(image));
            }

            WritePkm(destStream, mips[0], image.Width, image.Height);
        }

        public void ConvertFromAlAlt(AlImage image, Stream destStream)
        {
            if (image.PixelFormat != "EC1A")
                throw new ArgumentException("Pixel format does not have alternate representation.", nameof(image));

            // Give second half of the mips
            byte[] newMip = new byte[image.Mipmaps[0].Length / 2];
            Buffer.BlockCopy(image.Mipmaps[0], newMip.Length, newMip, 0, newMip.Length);

            WritePkm(destStream, newMip, image.Width, image.Height);
        }

        public bool CanConvert(string pixelFormat)
        {
            return pixelFormat == "EC1A" || pixelFormat == "ETC1";
        }

        public bool HasAlternativeFile(AlImage image)
        {
            return image.PixelFormat == "EC1A";
        }

        const ushort ETC1_RGB_NO_MIPMAPS = 0;

        static void WritePkm(Stream stream, byte[] data, uint width, uint height)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write("PKM 10".ToCharArray());
            WriteBeUInt16(bw, ETC1_RGB_NO_MIPMAPS);
            WriteBeUInt16(bw, (ushort)((width + 3) & ~3));
            WriteBeUInt16(bw, (ushort)((height + 3) & ~3));
            WriteBeUInt16(bw, (ushort)width);
            WriteBeUInt16(bw, (ushort)height);
            bw.Write(data);
        }

        static void WriteBeUInt16(BinaryWriter bw, ushort value)
        {
            bw.Write((byte)(value >> 8));
            bw.Write((byte)value);
        }
    }
}
