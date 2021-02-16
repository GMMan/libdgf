using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace GMWare.IO
{
    /// <summary>
    /// Provides some commonly used methods for Stream manipulation.
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// Opens a DeflateStream for reading Zlib compressed data from the current position of the <paramref name="stream"/> parameter. Closing this Stream will not close the underlying Stream.
        /// </summary>
        /// <param name="stream">The Stream to create a DeflateStream from.</param>
        /// <returns>The opened DeflateStream.</returns>
        public static DeflateStream OpenDeflateDecompressionStreamCheap(Stream stream)
        {
            return OpenDeflateDecompressionStreamCheap(stream, true);
        }

        /// <summary>
        /// Opens a DeflateStream for reading Zlib compressed data from the current position of the <paramref name="stream"/> parameter.
        /// </summary>
        /// <param name="stream">The Stream to create a DeflateStream from.</param>
        /// <param name="leaveOpen">Specifies whether or not the underlying Stream will be left open when this Stream is closed.</param>
        /// <returns>The opened DeflateStream.</returns>
        public static DeflateStream OpenDeflateDecompressionStreamCheap(Stream stream, bool leaveOpen)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            stream.ReadByte();
            stream.ReadByte();
            return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen);
        }

        /// <summary>
        /// Copies a number of bytes from one Stream to the other. The current position of each is used.
        /// </summary>
        /// <param name="src">The Stream to copy from.</param>
        /// <param name="dest">The Stream to copy to.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <returns>Whether or not all requested bytes are copied.</returns>
        [Obsolete("For backward compatibility only. Please use StreamCopy().")]
        public static bool StreamCopyWithLength(Stream src, Stream dest, int length)
        {
            return StreamCopy(src, dest, length);
        }

        /// <summary>
        /// Copies a number of bytes from one Stream to the other. The current position of each is used.
        /// </summary>
        /// <param name="src">The Stream to copy from.</param>
        /// <param name="dest">The Stream to copy to.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <returns>Whether or not all requested bytes are copied.</returns>
        public static bool StreamCopy(Stream src, Stream dest, long length)
        {
            return StreamCopy(src, dest, length, null);
        }

        /// <summary>
        /// Copies a number of bytes from one Stream to the other. The current position of each is used.
        /// </summary>
        /// <param name="src">The Stream to copy from.</param>
        /// <param name="dest">The Stream to copy to.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <param name="procDelegate">A delegate to process the read buffer before it's written to the destination.</param>
        /// <returns>Whether or not all requested bytes are copied.</returns>
        public static bool StreamCopy(Stream src, Stream dest, long length, StreamCopyProcessor procDelegate)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (dest == null) throw new ArgumentNullException("dest");

            if (length == 0) return true;

            const int BUFFER_SIZE = 4096;

            byte[] buffer = new byte[BUFFER_SIZE];
            int read;
            long left = length;
            bool continueProcessing = true;

            while (continueProcessing && left / buffer.Length != 0 && (read = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (procDelegate != null)
                {
                    continueProcessing = procDelegate(buffer, read);
                }
                dest.Write(buffer, 0, read);
                left -= read;
            }

            // Should stop if zero bytes have been read from stream although some should have been read
            if (length > BUFFER_SIZE && left == length) return false;

            if (src.CanSeek && src.Position == src.Length && left != 0) throw new EndOfStreamException();

            while (continueProcessing && left > 0 && (read = src.Read(buffer, 0, (int)left)) > 0)
            {
                if (procDelegate != null)
                {
                    continueProcessing = procDelegate(buffer, read);
                }
                dest.Write(buffer, 0, read);
                left -= read;
            }

            return left == 0;
        }

        /// <summary>
        /// Copies one Stream to the other. The current position of each is used.
        /// </summary>
        /// <param name="src">The Stream to copy from.</param>
        /// <param name="dest">The Stream to copy to.</param>
        /// <returns>The number of bytes copied.</returns>
        public static long StreamCopy(Stream src, Stream dest)
        {
            return StreamCopy(src, dest, null);
        }

        /// <summary>
        /// Copies one Stream to the other. The current position of each is used.
        /// </summary>
        /// <param name="src">The Stream to copy from.</param>
        /// <param name="dest">The Stream to copy to.</param>
        /// <param name="procDelegate">A delegate for processing a read chunk before it is written.</param>
        /// <returns>The number of bytes copied.</returns>
        public static long StreamCopy(Stream src, Stream dest, StreamCopyProcessor procDelegate)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (dest == null) throw new ArgumentNullException("dest");

            // From Stack Overflow, probably
            const int BUFFER_SIZE = 4096;

            byte[] buffer = new byte[BUFFER_SIZE];
            long bytesCopied = 0;
            int bytesRead;
            bool continueProcessing = true;

            do
            {
                bytesRead = src.Read(buffer, 0, BUFFER_SIZE);
                if (procDelegate != null)
                {
                    continueProcessing = procDelegate(buffer, bytesRead);
                }
                dest.Write(buffer, 0, bytesRead);
                bytesCopied += bytesRead;
            }
            while (continueProcessing && bytesRead != 0);
            return bytesCopied;
        }

        /// <summary>
        /// Encapsulates a method for processing bytes that are being copied.
        /// </summary>
        /// <param name="buffer">The bytes that have been read from the source stream and will be written to the destination stream</param>
        /// <param name="bytesRead">The number of bytes that have been read from the source stream stored in <paramref name="buffer"/></param>
        /// <returns></returns>
        public delegate bool StreamCopyProcessor(byte[] buffer, int bytesRead);
    }
}
