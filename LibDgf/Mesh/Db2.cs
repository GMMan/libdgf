using LibDgf.Ps2.Vif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Mesh
{
    public class Db2
    {
        BinaryReader br;

        public List<Db2Element> Elements { get; set; } = new List<Db2Element>();

        public void Read(BinaryReader br)
        {
            uint vifStreamLength = br.ReadUInt32();
            uint version = br.ReadUInt32();
            if (version != 2) throw new InvalidDataException("DB version is not 2.");
            uint reserved1 = br.ReadUInt32();
            uint reserved2 = br.ReadUInt32();
            //if (reserved1 != 0 || reserved2 != 0)
            //    System.Diagnostics.Debugger.Break();

            Elements.Clear();
            var startPos = br.BaseStream.Position;
            var endPos = startPos + vifStreamLength;
            this.br = br;
            try
            {
                ExpectNop();
                ExpectNop();
                ExpectStCycl();
                ExpectUnpackV4_32();

                while (br.BaseStream.Position < endPos)
                {
                    Db2Element element = new Db2Element();
                    element.ElementLength = br.ReadInt32();
                    element.Flags = br.ReadUInt32();
                    //if ((element.Flags & 0xFFFFE0E0) != 0)
                    //    System.Diagnostics.Debugger.Break();
                    element.TextureIndex = br.ReadInt32();
                    element.Reserved = br.ReadUInt32();
                    //if (element.Reserved != 0)
                    //    System.Diagnostics.Debugger.Break();

                    int numReglistWords = (int)(element.Flags & 0xf);
                    if (numReglistWords != 0)
                    {
                        element.GsRegs = br.ReadBytes((numReglistWords + 1) * 16);
                    }

                    element.GifTagFan = br.ReadBytes(16);
                    element.GifTagStrip = br.ReadBytes(16);

                    // Retrieve number of vertices from GIFtag NLOOP
                    int numVertices = element.VertexCount;
                    for (int i = 0; i < numVertices; ++i)
                    {
                        element.Vertices.Add(br.ReadV4_32());
                    }

                    ExpectUnpackV3_16((byte)numVertices);
                    for (int i = 0; i < numVertices; ++i)
                    {
                        element.VertexNormals.Add(new Tuple<double, double, double>(
                            Utils.Convert12BitFixedToDouble(br.ReadInt16()),
                            Utils.Convert12BitFixedToDouble(br.ReadInt16()),
                            Utils.Convert12BitFixedToDouble(br.ReadInt16())
                        ));
                    }
                    ReadAlign(startPos, 4);

                    // Do STs exist if no texture?
                    ExpectUnpackV2_16((byte)numVertices);
                    for (int i = 0; i < numVertices; ++i)
                    {
                        element.STCoordinates.Add(new Tuple<double, double>(
                            Utils.Convert12BitFixedToDouble(br.ReadInt16()),
                            Utils.Convert12BitFixedToDouble(br.ReadInt16())
                        ));
                    }
                    ReadAlign(startPos, 16); // Skipping over some NOPs in the process, assume that's what they are

                    ExpectMsCnt();

                    Elements.Add(element);

                    ExpectNop();
                    if (br.BaseStream.Position == endPos - 8)
                    {
                        ExpectNop();
                        ExpectNop();
                    }
                    else
                    {
                        ExpectStCycl();
                        ExpectUnpackV4_32();
                    }
                }
            }
            finally
            {
                this.br = null;
            }
        }

        void ReadAlign(long startPos, int bytes)
        {
            var aligned = (br.BaseStream.Position - startPos + bytes - 1) / bytes * bytes;
            br.BaseStream.Seek(startPos + aligned - br.BaseStream.Position, SeekOrigin.Current);
        }

        #region Sanity checking functions

        VifCode ExpectNop()
        {
            return ExpectVifCode(0x00000000, 0x7f000000);
        }

        VifCode ExpectStCycl()
        {
            return ExpectVifCode(0x01000404, 0x7f00ffff);
        }

        VifCodeUnpack ExpectUnpackV4_32()
        {
            return (VifCodeUnpack)ExpectVifCode(0x6c008000, 0x6f00c000);
        }

        VifCodeUnpack ExpectUnpackV3_16(byte num)
        {
            return (VifCodeUnpack)ExpectVifCode(0x69008000 | ((uint)num << 16), 0x6fffc000);
        }

        VifCodeUnpack ExpectUnpackV2_16(byte num)
        {
            return (VifCodeUnpack)ExpectVifCode(0x65008000 | ((uint)num << 16), 0x6fffc000);
        }

        VifCode ExpectMsCnt()
        {
            return ExpectVifCode(0x17000000, 0x7f000000);
        }

        VifCode ExpectVifCode(uint value, uint mask)
        {
            uint read = br.ReadUInt32();
            if ((read & mask) != value)
                throw new InvalidDataException($"VIFcode expectation failed at {br.BaseStream.Position - 4:x8}.");
            return new VifCode { Value = read };
        }

        #endregion
    }
}
