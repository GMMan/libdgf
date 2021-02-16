using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using GMWare.IO;

namespace LibDgf.Aqualead.Archive
{
    public class AlArchiveV2 : IDisposable
    {
        AlAarHeaderV2 header;
        List<AlAarEntryV2> entries;
        Stream stream;
        private bool disposedValue;

        public AlArchiveV2(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Load();
        }

        public IList<AlAarEntryV2> Entries
        {
            get
            {
                CheckDisposed();
                return new ReadOnlyCollection<AlAarEntryV2>(entries);
            }
        }

        void Load()
        {
            BinaryReader br = new BinaryReader(stream);
            if (new string(br.ReadChars(4)) != "ALAR")
                throw new InvalidDataException("Not an AquaLead archive.");
            if (br.ReadByte() != 2)
                throw new NotSupportedException("Not version 2 archive.");

            header = new AlAarHeaderV2();
            header.Read(br);

            entries = new List<AlAarEntryV2>();
            for (int i = 0; i < header.Count; ++i)
            {
                var entry = new AlAarEntryV2();
                entry.Read(br);
                entries.Add(entry);
            }

            foreach (var entry in entries)
            {
                if ((entry.Flags & AlAarEntryFlags.IsUseName) != 0)
                {
                    stream.Seek(entry.Offset - 0x22, SeekOrigin.Begin);
                    entry.Name = StringReadingHelper.ReadNullTerminatedStringFromFixedSizeBlock(br, 0x20, Encoding.UTF8);
                }

                if ((entry.Flags & ~AlAarEntryFlags.IsUseName) != 0)
                {
                    Console.WriteLine($"Entry {entry.Name} has other flags set: {entry.Flags}");
                    //Debugger.Break();
                }
            }
        }

        public Stream GetFile(AlAarEntryV2 entry)
        {
            if (!entries.Contains(entry))
                throw new ArgumentException("Entry not from this archive.", nameof(entry));
            if (stream == null) throw new InvalidOperationException("Archive is not opened from existing.");
            CheckDisposed();

            stream.Seek(entry.Offset, SeekOrigin.Begin);
            MemoryStream ms = new MemoryStream();
            StreamUtils.StreamCopy(stream, ms, entry.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return Utils.CheckDecompress(ms);
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
                    stream.Close();
                }

                entries = null;
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
