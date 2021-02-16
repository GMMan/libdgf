using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Dat
{
    public class DatReader : IDisposable
    {
        Stream stream;
        BinaryReader br;
        List<DatEntry> entries = new List<DatEntry>();
        private bool disposedValue;

        public DatReader(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

            br = new BinaryReader(stream);
            if (new string(br.ReadChars(4)) != "DAT\0") throw new InvalidDataException("Not a DAT file.");
            int numEntries = br.ReadInt32();
            for (int i = 0; i < numEntries; ++i)
            {
                DatEntry entry = new DatEntry();
                entry.Read(br);
                entries.Add(entry);
            }
        }

        public int EntriesCount
        {
            get
            {
                CheckDisposed();
                return entries.Count;
            }
        }

        public byte[] GetData(int index)
        {
            CheckDisposed();
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            if (index >= EntriesCount) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than or equal to count.");

            var entry = entries[index];
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            return br.ReadBytes((int)entry.Length);
        }

        public DatEntry GetEntry(int index)
        {
            CheckDisposed();
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            if (index >= EntriesCount) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than or equal to count.");

            var entry = entries[index];
            return new DatEntry
            {
                Offset = entry.Offset,
                Length = entry.Length
            };
        }

        void CheckDisposed()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
