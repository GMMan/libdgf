using LibDgf.Aqualead.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Image.Conversion
{
    public class PngConverter : IAlImageConverter
    {
        public string FileExtension => ".png";

        public void ConvertFromAl(AlImage image, Stream destStream)
        {
            // Only grab the first mip
            var pixels = image.Mipmaps[0];
            using (MemoryStream ms = new MemoryStream(pixels))
            {
                BinaryReader br = new BinaryReader(ms);
                using (var img = ConvertBgra32(br, (int)image.Width, (int)image.Height))
                {
                    img.SaveAsPng(destStream);
                }
            }
        }

        public bool CanConvert(string pixelFormat)
        {
            return pixelFormat == "BGRA";
        }

        public static Image<Bgra32> ConvertBgra32(BinaryReader br, int width, int height)
        {
            Image<Bgra32> img = new Image<Bgra32>(width, height);
            for (int y = 0; y < height; ++y)
            {
                var row = img.GetPixelRowSpan(y);
                for (int x = 0; x < width; ++x)
                {
                    byte b = br.ReadByte();
                    byte g = br.ReadByte();
                    byte r = br.ReadByte();
                    byte a = br.ReadByte();
                    row[x] = new Bgra32(r, g, b, a);
                }
            }
            return img;
        }

        public void ConvertFromAlAlt(AlImage image, Stream destStream)
        {
            throw new NotSupportedException();
        }

        public bool HasAlternativeFile(AlImage image)
        {
            return false;
        }
    }
}
