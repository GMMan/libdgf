using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public static class BinaryReaderVifExtensions
    {
        public static VuFloat ReadPs2Float(this BinaryReader br)
        {
            return new VuFloat { Packed = br.ReadUInt32() };
        }

        public static VuVector[] ReadBoundingBox(this BinaryReader br)
        {
            var box = new VuVector[8];
            for (int i = 0; i < box.Length; ++i)
            {
                box[i] = br.ReadV4_32();
            }
            return box;
        }

        public static VuVector ReadOneVifCodeUnpack(this BinaryReader br, VifCodeUnpack u)
        {
            switch (u.Vn)
            {
                case VifUnpackVnType.S:
                    switch (u.Vl)
                    {
                        case VifUnpackVlType.L_32:
                            return br.ReadS_32();
                        case VifUnpackVlType.L_16:
                            return u.Unsigned ? br.ReadS_16U() : br.ReadS_16S();
                        case VifUnpackVlType.L_8:
                            return u.Unsigned ? br.ReadS_8U() : br.ReadS_8S();
                        default:
                            break;
                    }
                    break;
                case VifUnpackVnType.V2:
                    switch (u.Vl)
                    {
                        case VifUnpackVlType.L_32:
                            return br.ReadV2_32();
                        case VifUnpackVlType.L_16:
                            return u.Unsigned ? br.ReadV2_16U() : br.ReadV2_16S();
                        case VifUnpackVlType.L_8:
                            return u.Unsigned ? br.ReadV2_8U() : br.ReadV2_8S();
                        default:
                            break;
                    }
                    break;
                case VifUnpackVnType.V3:
                    switch (u.Vl)
                    {
                        case VifUnpackVlType.L_32:
                            return br.ReadV3_32();
                        case VifUnpackVlType.L_16:
                            return u.Unsigned ? br.ReadV3_16U() : br.ReadV3_16S();
                        case VifUnpackVlType.L_8:
                            return u.Unsigned ? br.ReadV3_8U() : br.ReadV3_8S();
                        default:
                            break;
                    }
                    break;
                case VifUnpackVnType.V4:
                    switch (u.Vl)
                    {
                        case VifUnpackVlType.L_32:
                            return br.ReadV4_32();
                        case VifUnpackVlType.L_16:
                            return u.Unsigned ? br.ReadV4_16U() : br.ReadV4_16S();
                        case VifUnpackVlType.L_8:
                            return u.Unsigned ? br.ReadV4_8U() : br.ReadV4_8S();
                        case VifUnpackVlType.L_5:
                            return br.ReadV4_5();
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            throw new ArgumentException("Invalid vn/vl combination", nameof(u));
        }

        public static VuVector ReadV4_32(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadUInt32() },
                Y = new VuFloat { Packed = br.ReadUInt32() },
                Z = new VuFloat { Packed = br.ReadUInt32() },
                W = new VuFloat { Packed = br.ReadUInt32() }
            };
        }

        public static VuVector ReadV4_16U(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadUInt16() },
                Y = new VuFloat { Packed = br.ReadUInt16() },
                Z = new VuFloat { Packed = br.ReadUInt16() },
                W = new VuFloat { Packed = br.ReadUInt16() },
            };
        }

        public static VuVector ReadV4_16S(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
                Y = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
                Z = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
                W = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
            };
        }

        public static VuVector ReadV4_8U(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadByte() },
                Y = new VuFloat { Packed = br.ReadByte() },
                Z = new VuFloat { Packed = br.ReadByte() },
                W = new VuFloat { Packed = br.ReadByte() },
            };
        }

        public static VuVector ReadV4_8S(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
                Y = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
                Z = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
                W = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
            };
        }

        public static VuVector ReadV3_32(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadUInt32() },
                Y = new VuFloat { Packed = br.ReadUInt32() },
                Z = new VuFloat { Packed = br.ReadUInt32() },
            };
        }

        public static VuVector ReadV3_16U(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadUInt16() },
                Y = new VuFloat { Packed = br.ReadUInt16() },
                Z = new VuFloat { Packed = br.ReadUInt16() },
            };
        }

        public static VuVector ReadV3_16S(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
                Y = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
                Z = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) },
            };
        }

        public static VuVector ReadV3_8U(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = br.ReadByte() },
                Y = new VuFloat { Packed = br.ReadByte() },
                Z = new VuFloat { Packed = br.ReadByte() },
            };
        }

        public static VuVector ReadV3_8S(this BinaryReader br)
        {
            return new VuVector
            {
                X = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
                Y = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
                Z = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) },
            };
        }

        public static VuVector ReadV2_32(this BinaryReader br)
        {
            var x = new VuFloat { Packed = br.ReadUInt32() };
            var y = new VuFloat { Packed = br.ReadUInt32() };
            return new VuVector
            {
                X = x,
                Y = y,
                Z = x,
                W = y
            };
        }

        public static VuVector ReadV2_16U(this BinaryReader br)
        {
            var x = new VuFloat { Packed = br.ReadUInt16() };
            var y = new VuFloat { Packed = br.ReadUInt16() };
            return new VuVector
            {
                X = x,
                Y = y,
                Z = x,
                W = y
            };
        }

        public static VuVector ReadV2_16S(this BinaryReader br)
        {
            var x = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) };
            var y = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) };
            return new VuVector
            {
                X = x,
                Y = y,
                Z = x,
                W = y
            };
        }

        public static VuVector ReadV2_8U(this BinaryReader br)
        {
            var x = new VuFloat { Packed = (uint)br.ReadByte() };
            var y = new VuFloat { Packed = (uint)br.ReadByte() };
            return new VuVector
            {
                X = x,
                Y = y,
                Z = x,
                W = y
            };
        }

        public static VuVector ReadV2_8S(this BinaryReader br)
        {
            var x = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) };
            var y = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) };
            return new VuVector
            {
                X = x,
                Y = y,
                Z = x,
                W = y
            };
        }

        public static VuVector ReadS_32(this BinaryReader br)
        {
            var s = new VuFloat { Packed = br.ReadUInt32() };
            return new VuVector
            {
                X = s,
                Y = s,
                Z = s,
                W = s
            };
        }

        public static VuVector ReadS_16U(this BinaryReader br)
        {
            var s = new VuFloat { Packed = (uint)br.ReadUInt16() };
            return new VuVector
            {
                X = s,
                Y = s,
                Z = s,
                W = s
            };
        }

        public static VuVector ReadS_16S(this BinaryReader br)
        {
            var s = new VuFloat { Packed = unchecked((uint)br.ReadInt16()) };
            return new VuVector
            {
                X = s,
                Y = s,
                Z = s,
                W = s
            };
        }

        public static VuVector ReadS_8U(this BinaryReader br)
        {
            var s = new VuFloat { Packed = (uint)br.ReadByte() };
            return new VuVector
            {
                X = s,
                Y = s,
                Z = s,
                W = s
            };
        }

        public static VuVector ReadS_8S(this BinaryReader br)
        {
            var s = new VuFloat { Packed = unchecked((uint)br.ReadSByte()) };
            return new VuVector
            {
                X = s,
                Y = s,
                Z = s,
                W = s
            };
        }

        public static VuVector ReadV4_5(this BinaryReader br)
        {
            uint rgba = br.ReadUInt16();
            return new VuVector
            {
                X = new VuFloat { Packed = (rgba << 3) & 0xf8 },
                Y = new VuFloat { Packed = (rgba >> 2) & 0xf8 },
                Z = new VuFloat { Packed = (rgba >> 7) & 0xf8 },
                W = new VuFloat { Packed = (rgba >> 8) & 0x80 },
            };
        }
    }
}
