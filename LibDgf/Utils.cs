using LibDgf.Aqualead;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf
{
    public static class Utils
    {
        public static Stream CheckDecompress(Stream stream, bool keepOpen = false)
        {
            BinaryReader br = new BinaryReader(stream);
            if (new string(br.ReadChars(4)) == "ALLZ")
            {
                stream.Seek(-4, SeekOrigin.Current);
                var decoder = new AlLzDecoder();
                byte[] decoded = decoder.Decode(stream);
                if (!keepOpen) stream.Close();
                stream = new MemoryStream(decoded);
            }
            else
            {
                stream.Seek(-4, SeekOrigin.Current);
            }
            return stream;
        }

        public static double Convert12BitFixedToDouble(int value)
        {
            return value * 0.000244140625; // value / 2^12
        }
    }
}
