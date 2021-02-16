using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GMWare.IO
{
    /// <summary>
    /// Collection of methods for reading strings using BinaryReader
    /// </summary>
    public static class StringReadingHelper
    {
        /// <summary>
        /// Reads a string of a given length.
        /// </summary>
        /// <param name="reader">BinaryReader to read from</param>
        /// <param name="length">Length of string to read</param>
        /// <returns>The string that was read.</returns>
        public static string ReadLengthedString(BinaryReader reader, int length)
        {
            return new string(reader.ReadChars(length));
        }

        /// <summary>
        /// Reads a null terminated string.
        /// </summary>
        /// <param name="reader">BinaryReader to read from</param>
        /// <returns>The string that was read.</returns>
        public static string ReadNullTerminatedString(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            for (char ch = reader.ReadChar(); ch != '\0'; ch = reader.ReadChar())
            {
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads a null terminated string from within a fixed size block.
        /// </summary>
        /// <param name="reader">BinaryReader to read from</param>
        /// <param name="blockLen">The length of the fixed size block</param>
        /// <param name="encoding">The encoding the string is in</param>
        /// <returns>The string that was read.</returns>
        public static string ReadNullTerminatedStringFromFixedSizeBlock(BinaryReader reader, int blockLen, Encoding encoding)
        {
            byte[] data = reader.ReadBytes(blockLen);
            string str = encoding.GetString(data);
            int indNull = str.IndexOf('\0');
            if (indNull >= 0)
            {
                return str.Substring(0, indNull);
            }
            else
            {
                return str;
            }
        }
    }
}
