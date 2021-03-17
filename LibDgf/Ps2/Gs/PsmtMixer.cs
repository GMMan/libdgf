using LibDgf.Txm;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Gs
{
    public static class PsmtMixer
    {
        static int[,] PSMCT32_TO_PSMT4_COL02_LOOKUP = new int[16, 8];
        static int[,] PSMCT32_TO_PSMT4_COL13_LOOKUP = new int[16, 8];
        static int[,] PSMCT32_TO_PSMT8_COL02_LOOKUP = new int[16, 4];
        static int[,] PSMCT32_TO_PSMT8_COL13_LOOKUP = new int[16, 4];

        static PsmtMixer()
        {
            FillLookup(PSMCT32_TO_PSMT4_COL02_LOOKUP, false);
            FillOddLookup(PSMCT32_TO_PSMT4_COL02_LOOKUP, PSMCT32_TO_PSMT4_COL13_LOOKUP);
            FillLookup(PSMCT32_TO_PSMT8_COL02_LOOKUP, false);
            FillOddLookup(PSMCT32_TO_PSMT8_COL02_LOOKUP, PSMCT32_TO_PSMT8_COL13_LOOKUP);

            //PrintLookup(PSMCT32_TO_PSMT4_COL02_LOOKUP, nameof(PSMCT32_TO_PSMT4_COL02_LOOKUP));
            //PrintLookup(PSMCT32_TO_PSMT4_COL13_LOOKUP, nameof(PSMCT32_TO_PSMT4_COL13_LOOKUP));
            //PrintLookup(PSMCT32_TO_PSMT8_COL02_LOOKUP, nameof(PSMCT32_TO_PSMT8_COL02_LOOKUP));
            //PrintLookup(PSMCT32_TO_PSMT8_COL13_LOOKUP, nameof(PSMCT32_TO_PSMT8_COL13_LOOKUP));
        }

        static void PrintLookup(int[,] lookup, string name)
        {
            Console.WriteLine(name);
            int rowNum = lookup.GetLength(0);
            int colNum = lookup.GetLength(1);
            for (int row = 0; row < rowNum; ++row)
            {
                for (int col = colNum - 1; col >= 0; --col)
                {
                    Console.Write("{0,3} ", lookup[row, col]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void FillLookup(int[,] lookup, bool skipNonConsec)
        {
            int numCols = lookup.GetLength(1);
            int num = 0;

            // Phase 1: consecutive numbers
            // Fill every second column
            // Top half then bottom half
            for (int half = 0; half < 2; ++half)
            {
                for (int col = 0; col < numCols; col += skipNonConsec ? 1 : 2)
                {
                    for (int row = 0; row < 8; ++row)
                    {
                        lookup[half * 8 + row, col] = num++;
                    }
                }
            }

            // Phase 2: wrapped numbers
            if (!skipNonConsec)
            {
                for (int half = 0; half < 2; ++half)
                {
                    for (int col = 1; col < numCols; col += 2)
                    {
                        for (int row = 4; row < 12; ++row)
                        {
                            lookup[half * 8 + (row % 8), col] = num++;
                        }
                    }
                }
            }
        }

        static void FillOddLookup(int[,] evenLookup, int[,] oddLookup)
        {
            int numCols = evenLookup.GetLength(1);
            for (int half = 0; half < 2; ++half)
            {
                for (int i = 0; i < 8; ++i)
                {
                    for (int j = 0; j < numCols; ++j)
                    {
                        oddLookup[half * 8 + i, j] = evenLookup[half * 8 + ((i + 4) % 8), j];
                    }
                }
            }
        }

        public static byte[] MixColumn(byte[] column, TxmPixelFormat srcFormat, TxmPixelFormat destFormat, bool isOdd)
        {
            if (srcFormat != TxmPixelFormat.PSMCT32)
                throw new NotSupportedException("Only PSMCT32 supported as source format.");
            switch (destFormat)
            {
                case TxmPixelFormat.PSMT4:
                    return MixColumn32To4(column, isOdd);
                case TxmPixelFormat.PSMT8:
                    return MixColumn32To8(column, isOdd);
                default:
                    throw new NotSupportedException($"{destFormat} not supported as destination format.");
            }
        }

        static byte[] MixColumn32To4(byte[] column, bool isOdd)
        {
            int[,] lookup = isOdd ? PSMCT32_TO_PSMT4_COL13_LOOKUP : PSMCT32_TO_PSMT4_COL02_LOOKUP;
            int numCol = lookup.GetLength(1);
            byte[] dest = new byte[column.Length];
            byte b = 0;
            for (int i = 0; i < column.Length * 2; ++i)
            {
                if (i % 2 == 0)
                    b = column[i / 2];
                else
                    b >>= 4;

                int index = lookup[i / numCol, i % numCol];
                dest[index / 2] |= (byte)((b & 0x0f) << (index % 2 * 4));
            }
            return dest;
        }

        static byte[] MixColumn32To8(byte[] column, bool isOdd)
        {
            int[,] lookup = isOdd ? PSMCT32_TO_PSMT8_COL13_LOOKUP : PSMCT32_TO_PSMT8_COL02_LOOKUP;
            int numCol = lookup.GetLength(1);
            byte[] dest = new byte[column.Length];
            for (int i = 0; i < column.Length; ++i)
            {
                int index = lookup[i / numCol, i % numCol];
                dest[index] = column[i];
            }
            return dest;
        }
    }
}
