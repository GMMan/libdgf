using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead
{
    public class AlLzDecoder
    {
        uint bitBuffer;
        int numBitsRemaining;
        BinaryReader br;

        public byte[] Decode(Stream inStream)
        {
            br = new BinaryReader(inStream);
            numBitsRemaining = 0;

            if (new string(br.ReadChars(4)) != "ALLZ")
                throw new InvalidDataException("Not an Aqualead LZ compressed file.");
            byte version = br.ReadByte();
            if (version > 1)
                throw new InvalidDataException("Version too new.");

            byte minLookbackLengthBits = br.ReadByte();
            byte minLookbackPosBits = br.ReadByte();
            byte minLiteralLengthBits = br.ReadByte();
            uint decompressedLength = br.ReadUInt32();

            byte[] output = new byte[decompressedLength];
            int outPos = 0;

            if (version == 0) ReadBits(1); // Consume literal bit if v0
            bool hasLiteral = true;

            while (true)
            {
                if (hasLiteral)
                {
                    if (outPos >= decompressedLength) break;
                    int literalLength = (int)ReadEncodedNum(minLiteralLengthBits) + 1;
                    if (inStream.Read(output, outPos, literalLength) != literalLength)
                        throw new IOException("Could not read all bytes requested.");
                    outPos += literalLength;
                    //Console.WriteLine("Did literal");
                }

                if (outPos >= decompressedLength) break;
                int lookbackPos = (int)ReadEncodedNum(minLookbackPosBits) + 1;
                int lookbackLength = (int)ReadEncodedNum(minLookbackLengthBits) + 3;

                for (int i = 0; i < lookbackLength; ++i)
                {
                    output[outPos] = output[outPos - lookbackPos];
                    ++outPos;
                }
                //Console.WriteLine("Did lookback");

                if (outPos >= decompressedLength) break;
                hasLiteral = ReadBits(1) == 0;
                //Console.WriteLine("Has literal: " + hasLiteral);
            }

            br = null;
            return output;
        }

        uint ReadEncodedNum(int minNumBits)
        {
            int numExtBits = CountBits();
            uint num = ReadBits(minNumBits);
            if (numExtBits > 0)
            {
                // Length encoding
                // e e e e 0 | xxxx | yy
                // Number of es indicate exponent and number of bits for mantissa
                // Subsequently, ((2 ^ count(e)) - 1 + xxxx) | yy
                num |= (uint)(ReadBits(numExtBits) + (1 << numExtBits) - 1) << minNumBits;
            }
            return num;
        }

        void EnsureBits(int numBits)
        {
            while (numBitsRemaining < numBits)
            {
                if (numBitsRemaining + 8 > 32) throw new ArgumentOutOfRangeException(nameof(numBits));
                bitBuffer |= (uint)br.ReadByte() << numBitsRemaining;
                numBitsRemaining += 8;
            }
        }

        uint ReadBits(int numBits)
        {
            EnsureBits(numBits);
            uint output = (uint)(bitBuffer & ((1 << numBits) - 1));
            bitBuffer >>= numBits;
            numBitsRemaining -= numBits;
            return output;
        }

        int CountBits()
        {
            int i = 0;
            while (ReadBits(1) != 0) ++i;
            return i;
        }

        void WriteBits()
        {

        }
    }
}
