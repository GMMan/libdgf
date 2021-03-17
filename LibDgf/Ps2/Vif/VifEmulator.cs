using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public class VifEmulator
    {
        public delegate void VuMpgActivate(ushort? address, bool waitGif);

        VuVector[] memory;
        VifRegisters registers;
        bool maskPath3;

        public VifEmulator()
        {
            memory = new VuVector[1024];
            for (int i = 0; i < memory.Length; ++i)
            {
                memory[i] = new VuVector();
            }
            registers = new VifRegisters();
        }

        public VuVector[] VuMemory => memory;
        public VifRegisters Registers => registers;

        public void Process(BinaryReader br, int dataLength, VuMpgActivate onVuMpgActivate)
        {
            var startPos = br.BaseStream.Position;
            var endPos = startPos + dataLength;
            while (br.BaseStream.Position < endPos)
            {
                var vifcode = new VifCode { Value = br.ReadUInt32() };
                registers.Code = vifcode;
                //if ((vifcode.CmdWithoutInterrupt & VifCodeCmd.Unpack) != VifCodeCmd.Unpack)
                //{
                //    Console.WriteLine($"VIFcode: {vifcode.CmdWithoutInterrupt} NUM={vifcode.Num} IMMEDIATE={vifcode.Immediate:x4}");
                //}
                switch (vifcode.CmdWithoutInterrupt)
                {
                    case VifCodeCmd.Nop:
                        break;
                    case VifCodeCmd.StCycl:
                        registers.Cycle = vifcode.Immediate;
                        break;
                    case VifCodeCmd.Offset:
                        registers.Ofst = (uint)(vifcode.Immediate & 0x3ff);
                        registers.Stat_Dbf = false;
                        registers.TopS = registers.Base;
                        break;
                    case VifCodeCmd.Base:
                        registers.Base = (uint)(vifcode.Immediate & 0x3ff);
                        break;
                    case VifCodeCmd.Itop:
                        registers.ITopS = (uint)(vifcode.Immediate & 0x3ff);
                        break;
                    case VifCodeCmd.StMod:
                        registers.Mode = (uint)(vifcode.Immediate & 3);
                        break;
                    case VifCodeCmd.MskPath3:
                        maskPath3 = (vifcode.Immediate & 0x8000) != 0;
                        break;
                    case VifCodeCmd.Mark:
                        registers.Mark = vifcode.Immediate;
                        break;
                    case VifCodeCmd.FlushE:
                    case VifCodeCmd.Flush:
                    case VifCodeCmd.FlushA:
                        // Microprogram is run synchronously in emulation
                        break;
                    case VifCodeCmd.MsCal:
                        registers.DoubleBufferSwap();
                        onVuMpgActivate?.Invoke(vifcode.Immediate, false);
                        break;
                    case VifCodeCmd.MsCnt:
                        registers.DoubleBufferSwap();
                        onVuMpgActivate?.Invoke(null, false);
                        break;
                    case VifCodeCmd.MsCalF:
                        registers.DoubleBufferSwap();
                        onVuMpgActivate?.Invoke(vifcode.Immediate, true);
                        break;
                    case VifCodeCmd.StMask:
                        registers.Mask = br.ReadUInt32();
                        break;
                    case VifCodeCmd.StRow:
                        registers.R[0] = br.ReadUInt32();
                        registers.R[1] = br.ReadUInt32();
                        registers.R[2] = br.ReadUInt32();
                        registers.R[3] = br.ReadUInt32();
                        break;
                    case VifCodeCmd.StCol:
                        registers.C[0] = br.ReadUInt32();
                        registers.C[1] = br.ReadUInt32();
                        registers.C[2] = br.ReadUInt32();
                        registers.C[3] = br.ReadUInt32();
                        break;
                    case VifCodeCmd.Mpg:
                        {
                            if (!CheckAlignment(br, startPos, 8))
                                throw new InvalidDataException("MPG data is not aligned.");
                            //Console.WriteLine($"MPG load at 0x{vifcode.Immediate * 8:x4}");
                            // Skip MPG since we don't have a VU to execute it on
                            int skipLength = vifcode.Num;
                            if (skipLength == 0) skipLength = 256;
                            skipLength *= 8;
                            br.BaseStream.Seek(skipLength, SeekOrigin.Current);
                            break;
                        }
                    case VifCodeCmd.Direct:
                    case VifCodeCmd.DirectHl:
                        {
                            if (!CheckAlignment(br, startPos, 16))
                                throw new InvalidDataException("Direct data is not aligned.");
                            //Console.WriteLine($"Direct transfer");
                            // TODO: handle GIFtag
                            int skipLength = vifcode.Immediate;
                            if (skipLength == 0) skipLength = 65536;
                            skipLength *= 16;
                            br.BaseStream.Seek(skipLength, SeekOrigin.Current);
                            break;
                        }
                    default:
                        if ((vifcode.Cmd & VifCodeCmd.Unpack) == VifCodeCmd.Unpack)
                        {
                            ProcessVifCodeUnpack(br);
                            AlignReader(br, startPos, 4);
                            break;
                        }
                        else
                        {
                            throw new InvalidDataException("Invalid VIFcode command");
                        }
                }
            }
        }

        void ProcessVifCodeUnpack(BinaryReader br)
        {
            var vifcode = (VifCodeUnpack)registers.Code;
            registers.Num = vifcode.Num;
            //Console.WriteLine($"VIFcode: {VifCodeCmd.Unpack} vn={vifcode.Vn} vl={vifcode.Vl} NUM={vifcode.Num} ADDR={vifcode.Address:x4} FLG={vifcode.Flag} USN={vifcode.Unsigned} m={vifcode.Mask}");
            int addr = (int)((vifcode.Flag ? registers.TopS : 0) + vifcode.Address);
            int cycle = 0;
            bool isV4_5 = vifcode.Vn == VifUnpackVnType.V4 && vifcode.Vl == VifUnpackVlType.L_5;
            while (registers.Num > 0)
            {
                VuVector result = default;
                bool doSkip = false;
                bool doMode;
                bool doMask;
                if (registers.CycleCl >= registers.CycleWl)
                {
                    doMode = true;
                    doMask = false;
                    // Skipping write
                    if (cycle < registers.CycleWl)
                    {
                        // Write when under write limit
                        result = br.ReadOneVifCodeUnpack(vifcode);
                    }
                    
                    if (cycle == registers.CycleWl - 1)
                    {
                        doSkip = true;
                    }
                }
                else
                {
                    // Filling write
                    throw new NotImplementedException("Filling write not implemented");
                }

                // Write result
                result = ApplyMaskAndMode(result, doMode && !isV4_5, doMask);
                memory[addr++] = result;
                --registers.Num;

                // TODO: figure out the proper behavior for filling write
                if (doSkip)
                {
                    addr += registers.CycleCl - registers.CycleWl;
                    cycle = 0;
                }
                else
                {
                    ++cycle;
                }
            }
        }

        VuVector ApplyMaskAndMode(VuVector vector, bool doMode, bool doMask)
        {
            if (!doMode && !doMask) return vector;

            uint x = vector.X.Packed;
            uint y = vector.Y.Packed;
            uint z = vector.Z.Packed;
            uint w = vector.W.Packed;

            if (doMask)
            {
                throw new NotImplementedException("Masking not implemented");
            }

            if (doMode)
            {
                switch (registers.ModeMod)
                {
                    case VifMode.None:
                        break;
                    default:
                        throw new NotImplementedException("Addition decompression write not implemented");
                }
            }

            return new VuVector
            {
                X = new VuFloat { Packed = x },
                Y = new VuFloat { Packed = y },
                Z = new VuFloat { Packed = z },
                W = new VuFloat { Packed = w },
            };
        }

        // Alignment in bytes
        static void AlignReader(BinaryReader br, long startPos, int alignment)
        {
            var read = br.BaseStream.Position - startPos;
            var aligned = (read + alignment - 1) / alignment * alignment;
            br.BaseStream.Seek(aligned - read, SeekOrigin.Current);
        }

        static bool CheckAlignment(BinaryReader br, long startPos, int alignment)
        {
            var read = br.BaseStream.Position - startPos;
            var aligned = (read + alignment - 1) / alignment * alignment;
            return read == aligned;
        }
    }
}
