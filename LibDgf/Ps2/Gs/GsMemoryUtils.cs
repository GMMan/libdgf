using LibDgf.Txm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Ps2.Gs
{
    public static class GsMemoryUtils
    {
        public const int BYTES_PER_COLUMN = 64;
        public const int COLUMNS_PER_BLOCK = 4;
        public const int BYTES_PER_BLOCK = BYTES_PER_COLUMN * COLUMNS_PER_BLOCK;
        public const int BLOCKS_PER_PAGE = 32;
        public const int BYTES_PER_PAGE = BYTES_PER_BLOCK * BLOCKS_PER_PAGE;
        public const int TOTAL_PAGES = 512;
        public const int TOTAL_BYTES = BYTES_PER_PAGE * TOTAL_PAGES;

        public class ColumnParams
        {
            public int Width { get; }
            public int Height { get; }
            public int BitsPerPixel { get; }
            public int ColsPerPage { get; }
            public int RowsPerPage { get; }

            internal ColumnParams(int width, int height, int bitsPerPixel, int colsPerPage, int rowsPerPage)
            {
                Width = width;
                Height = height;
                BitsPerPixel = bitsPerPixel;
                ColsPerPage = colsPerPage;
                RowsPerPage = rowsPerPage;
            }
        }

        static readonly ColumnParams PSMCT32_COLUMN_PARAMS = new ColumnParams(8, 2, 32, 8, 4);
        static readonly ColumnParams PSMT8_COLUMN_PARAMS = new ColumnParams(16, 4, 8, 8, 4);
        static readonly ColumnParams PSMT4_COLUMN_PARAMS = new ColumnParams(32, 4, 4, 4, 8);

        public static ColumnParams GetColumnParams(TxmPixelFormat format)
        {
            switch (format)
            {
                case TxmPixelFormat.PSMCT32:
                case TxmPixelFormat.PSMCT24:
                    return PSMCT32_COLUMN_PARAMS;
                case TxmPixelFormat.PSMT8:
                    return PSMT8_COLUMN_PARAMS;
                case TxmPixelFormat.PSMT4:
                    return PSMT4_COLUMN_PARAMS;
                default:
                    throw new NotSupportedException($"{format} not supported.");
            }
        }

        public static byte[] ReadColumn(BinaryReader br, TxmPixelFormat format, int bytesPerLine)
        {
            ColumnParams colParams = GetColumnParams(format);
            byte[] data = new byte[colParams.Width * colParams.Height * colParams.BitsPerPixel / 8];
            int bytesPerDestLine = colParams.Width * colParams.BitsPerPixel / 8;
            int skipLength = bytesPerLine - bytesPerDestLine;
            for (int i = 0; i < colParams.Height; ++i)
            {
                byte[] lineData = br.ReadBytes(bytesPerDestLine);
                br.BaseStream.Seek(skipLength, SeekOrigin.Current);
                Buffer.BlockCopy(lineData, 0, data, i * bytesPerDestLine, lineData.Length);
            }
            return data;
        }

        public static void WriteColumn(BinaryWriter bw, TxmPixelFormat format, int bytesPerLine, byte[] data)
        {
            ColumnParams colParams = GetColumnParams(format);
            int bytesPerDestLine = colParams.Width * colParams.BitsPerPixel / 8;
            int skipLength = bytesPerLine - bytesPerDestLine;
            for (int i = 0; i < colParams.Height; ++i)
            {
                bw.Write(data, i * bytesPerDestLine, bytesPerDestLine);
                bw.Seek(skipLength, SeekOrigin.Current);
            }
        }

        // --------------------------------------------------------------------

        // Block address to linear offset lookup for PSMCT32
        static readonly int[,] PSMCT32_BLOCK_LOOKUP = new[,]
        {
            { 0, 1, 4, 5, 16, 17, 20, 21 },
            { 2, 3, 6, 7, 18, 19, 22, 23 },
            { 8, 9, 12, 13, 24, 25, 28, 29 },
            { 10, 11, 14, 15, 26, 27, 30, 31 }
        };

        static readonly int[,] PSMT8_BLOCK_LOOKUP = new[,]
        {
            { 0, 1, 4, 5, 16, 17, 20, 21 },
            { 2, 3, 6, 7, 18, 19, 22, 23 },
            { 8, 9, 12, 13, 24, 25, 28, 29 },
            { 10, 11, 14, 15, 26, 27, 30, 31 }
        };

        static readonly int[,] PSMT4_BLOCK_LOOKUP = new[,]
        {
            { 0, 2, 8, 10 },
            { 1, 3, 9, 11 },
            { 4, 6, 12, 14 },
            { 5, 7, 13, 15 },
            { 16, 18, 24, 26 },
            { 17, 19, 25, 27 },
            { 20, 22, 28, 30 },
            { 21, 23, 29, 31 }
        };

        static readonly int[] PSMCT32_BLOCK_REVERSE_LOOKUP;
        static readonly int[] PSMT8_BLOCK_REVERSE_LOOKUP;
        static readonly int[] PSMT4_BLOCK_REVERSE_LOOKUP;

        static GsMemoryUtils()
        {
            PSMCT32_BLOCK_REVERSE_LOOKUP = MakeReverseLookup(PSMCT32_BLOCK_LOOKUP);
            PSMT8_BLOCK_REVERSE_LOOKUP = MakeReverseLookup(PSMT8_BLOCK_LOOKUP);
            PSMT4_BLOCK_REVERSE_LOOKUP = MakeReverseLookup(PSMT4_BLOCK_LOOKUP);
        }

        static int[] MakeReverseLookup(int[,] lut)
        {
            int[] reverse = new int[lut.Length];
            int i = 0;
            foreach (var n in lut)
            {
                reverse[n] = i++;
            }
            return reverse;
        }

        public static int CalcBlockNumber(TxmPixelFormat format, int blockX, int blockY, int texBufWidth)
        {
            ColumnParams colParams = GetColumnParams(format);
            switch (format)
            {
                case TxmPixelFormat.PSMCT32:
                    return CalcBlockNumber(colParams, PSMCT32_BLOCK_LOOKUP,blockX, blockY, texBufWidth);
                case TxmPixelFormat.PSMT8:
                    return CalcBlockNumber(colParams, PSMT8_BLOCK_LOOKUP, blockX, blockY, texBufWidth);
                case TxmPixelFormat.PSMT4:
                    return CalcBlockNumber(colParams, PSMT4_BLOCK_LOOKUP, blockX, blockY, texBufWidth);
                default:
                    throw new NotSupportedException($"{format} not supported");
            }
        }

        public static int CalcBlockMemoryOffset(TxmPixelFormat format, int index)
        {
            ColumnParams colParams = GetColumnParams(format);
            switch (format)
            {
                case TxmPixelFormat.PSMCT32:
                    return CalcBlockMemoryOffset(colParams, PSMCT32_BLOCK_REVERSE_LOOKUP, index);
                case TxmPixelFormat.PSMT8:
                    return CalcBlockMemoryOffset(colParams, PSMT8_BLOCK_REVERSE_LOOKUP, index);
                case TxmPixelFormat.PSMT4:
                    return CalcBlockMemoryOffset(colParams, PSMT4_BLOCK_REVERSE_LOOKUP, index);
                default:
                    throw new NotSupportedException($"{format} not supported");
            }
        }

        static int CalcBlockNumber(ColumnParams colParams, int[,] lut, int blockX, int blockY, int texBufWidth)
        {
            int pageX = blockX / colParams.ColsPerPage;
            int pageY = blockY / colParams.RowsPerPage;
            int blockXInPage = blockX % colParams.ColsPerPage;
            int blockYInPage = blockY % colParams.RowsPerPage;

            return (pageY * texBufWidth + pageX) * BLOCKS_PER_PAGE + lut[blockYInPage, blockXInPage];
        }

        static int CalcBlockMemoryOffset(ColumnParams colParams, int[] lut, int index)
        {
            int pageIndex = index / BLOCKS_PER_PAGE;
            int rem = index % BLOCKS_PER_PAGE;
            int pixelsPerBlock = colParams.Width * colParams.Height * COLUMNS_PER_BLOCK;
            int memBlockNum = lut[rem];
            int fullBlockRows = memBlockNum / colParams.ColsPerPage;
            int partialBlocks = memBlockNum % colParams.ColsPerPage;

            return (
                pageIndex * BLOCKS_PER_PAGE * pixelsPerBlock + // Full pages
                fullBlockRows * pixelsPerBlock * colParams.ColsPerPage + // Full row of blocks
                partialBlocks * colParams.Width // Partial row of blocks
            ) * colParams.BitsPerPixel / 8;
        }

        public static int CalcTxmImageOffset(ColumnParams colParams, int blockIndex, int imageWidth)
        {
            int blocksPerRow = imageWidth / colParams.Width;
            if (blocksPerRow == 0) blocksPerRow = 1;
            int fullRows = blockIndex / blocksPerRow;
            int rowBlockIndex = blockIndex % blocksPerRow;
            return (
                fullRows * imageWidth * colParams.Height * 4 +
                rowBlockIndex * colParams.Width
            ) * colParams.BitsPerPixel / 8;
        }
    }
}
