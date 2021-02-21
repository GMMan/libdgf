using LibDgf.Dat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Font
{
    public class FontPack
    {
        const int WIDTH = 24;
        const int HEIGHT = 22;
        const int PADDING = 16;

        Dictionary<char, byte[]> characterMap = new Dictionary<char, byte[]>();

        public int Count => characterMap.Count;
        public IReadOnlyCollection<char> Characters => characterMap.Keys;

        public byte[] this[char ch]
        {
            get
            {
                return characterMap[ch];
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length != (WIDTH * HEIGHT + PADDING) / 2)
                    throw new ArgumentException("Pixels has incorrect length.", nameof(value));
                characterMap[ch] = value;
            }
        }

        public bool Remove(char ch)
        {
            return characterMap.Remove(ch);
        }

        public void Clear()
        {
            characterMap.Clear();
        }

        public void Read(Stream stream)
        {
            Encoding shiftJisEncoding = Encoding.GetEncoding(932);
            Clear();
            DatReader dat = new DatReader(stream);
            using (MemoryStream ms = new MemoryStream(dat.GetData(0)))
            {
                BinaryReader br = new BinaryReader(ms);
                uint infoSize = br.ReadUInt32();
                uint[] numChars = new uint[4];
                // numChars[0] is dummy
                for (int i = 1; i < numChars.Length; ++i)
                {
                    numChars[i] = br.ReadUInt32();
                }

                byte[] chBytes = new byte[2];
                for (int i = 1; i < numChars.Length; ++i)
                {
                    using MemoryStream graphicsMs = new MemoryStream(dat.GetData(i));
                    BinaryReader graphicsBr = new BinaryReader(graphicsMs);
                    var numCharInSection = numChars[i];
                    for (int j = 0; j < numCharInSection; ++j)
                    {
                        // Characters are stored as 16-bit little-endian values
                        chBytes[1] = br.ReadByte();
                        chBytes[0] = br.ReadByte();
                        char ch = shiftJisEncoding.GetChars(chBytes)[0];
                        byte[] pixels = graphicsBr.ReadBytes((WIDTH * HEIGHT + PADDING) / 2); // 4bpp
                        characterMap.Add(ch, pixels);
                    }
                }
            }
        }

        public void Write(Stream stream)
        {
            Encoding shiftJisEncoding = Encoding.GetEncoding(932);
            List<string> tempFiles = new List<string>();
            try
            {
                ushort[] charRanges = new ushort[] { 0x0, 0x889f, 0x989f }; // General, Kanji 1, Kanji 2
                uint[] charCounts = new uint[charRanges.Length];

                // Build mapping from Unicode to Shift-JIS
                List<Tuple<char, ushort>> encodedMapping = new List<Tuple<char, ushort>>();
                char[] chArray = new char[1];
                foreach (var ch in Characters)
                {
                    chArray[0] = ch;
                    byte[] encodedBytes = shiftJisEncoding.GetBytes(chArray);
                    ushort encoded = (ushort)((encodedBytes[0] << 8) | encodedBytes[1]);
                    encodedMapping.Add(new Tuple<char, ushort>(ch, encoded));
                }
                encodedMapping.Sort((x, y) => x.Item2.CompareTo(y.Item2));

                string infoFsPath = Path.GetTempFileName();
                tempFiles.Add(infoFsPath);
                int currentRange = -1;
                FileStream currentRangeFs = null;
                BinaryWriter currentRangeBw = null;
                DatBuilder datBuilder = new DatBuilder();
                datBuilder.ReplacementEntries.Add(new DatBuilder.ReplacementEntry
                {
                    Index = 0,
                    SourceFile = infoFsPath
                });
                using (FileStream infoFs = File.Create(infoFsPath))
                {
                    BinaryWriter infoBw = new BinaryWriter(infoFs);
                    infoBw.Write(new byte[4 + 4 * charCounts.Length]); // Dummy header

                    try
                    {
                        foreach (var pair in encodedMapping)
                        {
                            // Advance range if char matches next range's start
                            if (currentRange < charRanges.Length - 1 && pair.Item2 >= charRanges[currentRange + 1])
                            {
                                string path = Path.GetTempFileName();
                                tempFiles.Add(path);
                                if (currentRangeFs != null) currentRangeFs.Close();
                                currentRangeFs = File.Create(path);
                                currentRangeBw = new BinaryWriter(currentRangeFs);
                                ++currentRange;

                                datBuilder.ReplacementEntries.Add(new DatBuilder.ReplacementEntry
                                {
                                    Index = currentRange + 1,
                                    SourceFile = path
                                });
                            }

                            infoBw.Write(pair.Item2);
                            currentRangeBw.Write(this[pair.Item1]);
                            ++charCounts[currentRange];
                        }
                    }
                    finally
                    {
                        if (currentRangeFs != null) currentRangeFs.Close();
                    }

                    uint length = (uint)infoFs.Length;
                    // Pad to nearest 16 bytes
                    var paddedLength = (length + 15) & ~15;
                    infoBw.Write(new byte[paddedLength - length]);
                    infoFs.Seek(0, SeekOrigin.Begin);
                    infoBw.Write(length);
                    foreach (var count in charCounts)
                    {
                        infoBw.Write(count);
                    }
                }

                datBuilder.Build(stream);
            }
            finally
            {
                foreach (var file in tempFiles)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
