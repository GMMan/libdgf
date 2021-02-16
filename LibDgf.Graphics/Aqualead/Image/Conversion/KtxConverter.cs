using LibDgf.Aqualead.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KtxSharp;

namespace LibDgf.Aqualead.Image.Conversion
{
    public class KtxConverter : IAlImageConverter
    {
        public string FileExtension => ".ktx";

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
                foreach (var mip in image.Mipmaps)
                {
                    byte[] newMip = new byte[mip.Length / 2];
                    Buffer.BlockCopy(mip, 0, newMip, 0, newMip.Length);
                    mips.Add(newMip);
                }
            }
            else
            {
                throw new ArgumentException("Pixel format not supported.", nameof(image));
            }
            var ktx = KtxCreator.Create(GlDataType.Compressed, GlPixelFormat.GL_RGB, GlInternalFormat.GL_ETC1_RGB8_OES,
                image.Width, image.Height, mips, new Dictionary<string, MetadataValue>());
            KtxWriter.WriteTo(ktx, destStream);
        }

        public void ConvertFromAlAlt(AlImage image, Stream destStream)
        {
            if (image.PixelFormat != "EC1A")
                throw new ArgumentException("Pixel format does not have alternate representation.", nameof(image));

            // Give second half of the mips
            var mips = new List<byte[]>();
            foreach (var mip in image.Mipmaps)
            {
                byte[] newMip = new byte[mip.Length / 2];
                Buffer.BlockCopy(mip, newMip.Length, newMip, 0, newMip.Length);
                mips.Add(newMip);
            }

            var ktx = KtxCreator.Create(GlDataType.Compressed, GlPixelFormat.GL_RGB, GlInternalFormat.GL_ETC1_RGB8_OES,
                image.Width, image.Height, mips, new Dictionary<string, MetadataValue>());
            KtxWriter.WriteTo(ktx, destStream);
        }

        public bool CanConvert(string pixelFormat)
        {
            return pixelFormat == "EC1A" || pixelFormat == "ETC1";
        }

        public bool HasAlternativeFile(AlImage image)
        {
            return image.PixelFormat == "EC1A";
        }
    }
}
