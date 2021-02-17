using LibDgf.Txm;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Graphics
{
    public static class TxmConversion
    {
        public static void ConvertImageToTxm(string inPath, Stream outStream, byte level = 1, short bufferBase = 0, short paletteBufferBase = 0)
        {
            using (var image = Image.Load<Rgba32>(inPath))
            {
                // Gather all colors to see if it would fit in PSMT8
                TxmPixelFormat pixelFormat = TxmPixelFormat.None;
                HashSet<Rgba32> colorSet = new HashSet<Rgba32>();
                List<Rgba32> palette = null;
                for (int y = 0; y < image.Height; ++y)
                {
                    var row = image.GetPixelRowSpan(y);
                    for (int x = 0; x < image.Width; ++x)
                    {
                        colorSet.Add(row[x]);
                        if (colorSet.Count > 256)
                        {
                            pixelFormat = TxmPixelFormat.PSMCT32;
                            y = image.Height;
                            break;
                        }
                    }
                }

                short paletteWidth = 0;
                short paletteHeight = 0;
                if (pixelFormat == TxmPixelFormat.None)
                {
                    // Palette check passed, assign palettized pixel format
                    if (colorSet.Count > 16)
                    {
                        pixelFormat = TxmPixelFormat.PSMT8;
                        paletteWidth = 16;
                        paletteHeight = 16;
                    }
                    else
                    {
                        pixelFormat = TxmPixelFormat.PSMT4;
                        paletteWidth = 8;
                        paletteHeight = 2;
                    }
                    palette = new List<Rgba32>(colorSet);
                }

                // Write header
                BinaryWriter bw = new BinaryWriter(outStream);
                TxmHeader txmHeader = new TxmHeader
                {
                    ImageSourcePixelFormat = pixelFormat,
                    ImageVideoPixelFormat = pixelFormat,
                    ImageWidth = (short)image.Width,
                    ImageHeight = (short)image.Height,
                    ImageBufferBase = bufferBase,
                    ClutPixelFormat = palette != null ? TxmPixelFormat.PSMCT32 : TxmPixelFormat.None,
                    Misc = (byte)(level & 0x0f),
                    ClutWidth = paletteWidth,
                    ClutHeight = paletteHeight,
                    ClutBufferBase = paletteBufferBase
                };
                txmHeader.Write(bw);

                // Write palette
                int palettePixelsWritten = 0;
                if (pixelFormat == TxmPixelFormat.PSMT4)
                {
                    foreach (var color in palette)
                    {
                        bw.Write(color.R);
                        bw.Write(color.G);
                        bw.Write(color.B);
                        bw.Write((byte)((color.A + 1) >> 1));
                        ++palettePixelsWritten;
                    }
                }
                else if (pixelFormat == TxmPixelFormat.PSMT8)
                {
                    int baseOffset = 0;
                    Rgba32 black = new Rgba32();

                    int[] order = new int[]
                    {
                        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                        0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                        0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                        0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
                    };

                    while (palettePixelsWritten < palette.Count)
                    {
                        foreach (var offset in order)
                        {
                            var palOffset = baseOffset + offset;
                            var color = palOffset < palette.Count ? palette[palOffset] : black;
                            bw.Write(color.R);
                            bw.Write(color.G);
                            bw.Write(color.B);
                            bw.Write((byte)((color.A + 1) >> 1));
                            ++palettePixelsWritten;
                        }

                        baseOffset += order.Length;
                    }
                }

                // Pad out rest of palette
                int targetOffset = 16 + (txmHeader.GetClutByteSize() + 15) / 16 * 16;
                while (outStream.Position < targetOffset)
                {
                    bw.Write((byte)0);
                }

                // Write main image data
                byte pal4BppBuffer = 0;
                bool odd = false;
                for (int y = 0; y < image.Height; ++y)
                {
                    var row = image.GetPixelRowSpan(y);
                    for (int x = 0; x < image.Width; ++x)
                    {
                        var pixel = row[x];
                        if (pixelFormat == TxmPixelFormat.PSMCT32)
                        {
                            bw.Write(pixel.R);
                            bw.Write(pixel.G);
                            bw.Write(pixel.B);
                            bw.Write(pixel.A); // Should be halved, but full range is used on PC
                        }
                        else
                        {
                            var palIndex = palette.IndexOf(pixel);
                            if (pixelFormat == TxmPixelFormat.PSMT4)
                            {
                                pal4BppBuffer <<= 4;
                                pal4BppBuffer |= (byte)(palIndex & 0x0f);
                                odd = !odd;
                                if (!odd)
                                {
                                    bw.Write(pal4BppBuffer);
                                    pal4BppBuffer = 0;
                                }
                            }
                            else
                            {
                                bw.Write((byte)palIndex);
                            }
                        }
                    }
                }
            }
        }

        public static void ConvertTxmToPng(Stream stream, string outPath)
        {
            BinaryReader br = new BinaryReader(stream);
            TxmHeader imageHeader = new TxmHeader();
            imageHeader.Read(br);

            Console.WriteLine(imageHeader);
            if (imageHeader.Misc != 1)
                Console.WriteLine("Different level!");

            Image<Rgba32> image;
            if (imageHeader.ImageSourcePixelFormat == TxmPixelFormat.PSMT8 || imageHeader.ImageSourcePixelFormat == TxmPixelFormat.PSMT4)
            {
                Rgba32[] palette = null;
                if (imageHeader.ClutPixelFormat == TxmPixelFormat.PSMCT32)
                {
                    stream.Seek(16, SeekOrigin.Begin);
                    palette = GetRgba32Palette(br, imageHeader.ClutWidth, imageHeader.ClutHeight);
                    //fs.Seek(16, SeekOrigin.Begin);
                    //using (var palImage = ConvertTxmRgba32(br, imageHeader.ClutWidth, imageHeader.ClutHeight))
                    //{
                    //    palImage.SaveAsPng(Path.ChangeExtension(outPath, ".pal.png"));
                    //}
                }
                else
                {
                    throw new NotSupportedException("Unsupported pixel format from second texture");
                }

                stream.Seek(16 + (imageHeader.GetClutByteSize() + 15) / 16 * 16, SeekOrigin.Begin);
                if (imageHeader.ImageSourcePixelFormat == TxmPixelFormat.PSMT8)
                {
                    image = ConvertTxmIndexed8bpp(br, imageHeader.ImageWidth, imageHeader.ImageHeight, palette);
                }
                else
                {
                    image = ConvertTxmIndexed4bpp(br, imageHeader.ImageWidth, imageHeader.ImageHeight, palette);
                }
            }
            else if (imageHeader.ImageSourcePixelFormat == TxmPixelFormat.PSMCT32)
            {
                stream.Seek(16, SeekOrigin.Begin);
                image = ConvertTxmRgba32(br, imageHeader.ImageWidth, imageHeader.ImageHeight);
            }
            else
            {
                throw new NotSupportedException("Unsupported pixel format");
            }

            image.SaveAsPng(outPath);
            image.Dispose();
        }

        public static Image<Rgba32> ConvertTxmIndexed8bpp(BinaryReader br, int width, int height, Rgba32[] palette)
        {
            Image<Rgba32> img = new Image<Rgba32>(width, height);
            for (int y = 0; y < height; ++y)
            {
                var row = img.GetPixelRowSpan(y);
                for (int x = 0; x < width; ++x)
                {
                    byte index = br.ReadByte();
                    row[x] = palette[index];
                }
            }
            return img;
        }

        public static Image<Rgba32> ConvertTxmRgba32(BinaryReader br, int width, int height)
        {
            Image<Rgba32> img = new Image<Rgba32>(width, height);
            for (int y = 0; y < height; ++y)
            {
                var row = img.GetPixelRowSpan(y);
                for (int x = 0; x < width; ++x)
                {
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();
                    //int a = br.ReadByte() * 2;
                    //if (a > 255) a = 255;
                    byte a = br.ReadByte(); // Should be doubled, but full scale is used on PC
                    row[x] = new Rgba32(r, g, b, (byte)a);
                }
            }
            return img;
        }


        public static Image<Rgba32> ConvertTxmIndexed4bpp(BinaryReader br, int width, int height, Rgba32[] palette)
        {
            Image<Rgba32> img = new Image<Rgba32>(width, height);
            bool odd = false;
            byte index = 0;
            for (int y = 0; y < height; ++y)
            {
                var row = img.GetPixelRowSpan(y);
                for (int x = 0; x < width; ++x)
                {
                    if (!odd)
                    {
                        index = br.ReadByte();
                    }
                    row[x] = palette[index & 0xf];
                    index >>= 4;
                    odd = !odd;
                }
            }
            return img;
        }

        static Rgba32[] GetRgba32Palette(BinaryReader br, int width, int height)
        {
            int count = width * height;
            Rgba32[] colors = new Rgba32[count];
            for (int i = 0; i < count; ++i)
            {
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                int a = br.ReadByte() * 2;
                if (a > 255) a = 255;
                colors[i] = new Rgba32(r, g, b, (byte)a);
            }

            if (width == 8 && height == 2) return colors;

            // Reorder by column, left to right and up to down
            Rgba32[] reorderedColors = new Rgba32[count];
            int j = 0;
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int iy = 0; iy < 2; ++iy)
                    {
                        int offset = (y + iy) * width + x;
                        for (int ix = 0; ix < 8; ++ix)
                        {
                            reorderedColors[j++] = colors[offset + ix];
                        }
                    }
                }
            }

            return reorderedColors;
        }
    }
}
