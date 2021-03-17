using LibDgf.Ps2.Vif;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Mesh
{
    public class Db2Element
    {
        // Size of element in 128-bit words, including header, excluding surrounding VIFcodes
        public int ElementLength { get; set; }
        // 0x000f: number of REGLIST words (excluding leading GIFtag)
        // 0x0010: has texture
        // 0x00e0: reserved
        // 0x0100: has vertex color (deprecated, probably)
        // 0x0e00: GIFtag index - with standard REGLIST primitive: 3 = triangle fan, 4 = triangle strip
        // 0x1000: disable lighting
        // 0x80000: textures enabled (only set at runtime)
        public uint Flags { get; set; }
        public int TextureIndex { get; set; } // Actually byte
        public uint Reserved { get; set; }

        public byte[] GsRegs { get; set; } // One GS primitive, CLAMP_1 and TEX0_1
        public byte[] GifTagFan { get; set; }
        public byte[] GifTagStrip { get; set; }

        public List<VuVector> Vertices { get; } = new List<VuVector>();
        public List<Tuple<double, double, double>> VertexNormals { get; } = new List<Tuple<double, double, double>>();
        public List<Tuple<double, double>> STCoordinates { get; } = new List<Tuple<double, double>>();

        // Flags broken out

        public bool HasTexture
        {
            get
            {
                return (Flags & 0x10) != 0;
            }
            set
            {
                Flags = (uint)((Flags & ~0x10) | ((value ? 1u : 0) << 4));
            }
        }

        public int GifTagIndex
        {
            get
            {
                return (int)((Flags >> 9) & 7);
            }
            set
            {
                Flags = (uint)((Flags & ~0xe00) | (((uint)value & 7) << 9));
            }
        }

        public bool DisableLighting
        {
            get
            {
                return (Flags & 0x1000) != 0;
            }
            set
            {
                Flags = (uint)((Flags & ~0x1000) | ((value ? 1u : 0) << 12));
            }
        }

        public int VertexCount
        {
            get
            {
                return BitConverter.ToInt32(GifTagStrip, 0) & 0x7ff;
            }
            set
            {
                int num = BitConverter.ToInt32(GifTagStrip, 0);
                num = (num & ~0x7ff) | (value & 0x7ff);
                byte[] bytes = BitConverter.GetBytes(num);
                Buffer.BlockCopy(bytes, 0, GifTagStrip, 0, bytes.Length);

                num = BitConverter.ToInt32(GifTagFan, 0);
                num = (num & ~0x7ff) | (value & 0x7ff);
                bytes = BitConverter.GetBytes(num);
                Buffer.BlockCopy(bytes, 0, GifTagFan, 0, bytes.Length);
            }
        }
    }
}
