using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Dat
{
    public class DatBuilder
    {
        public class ReplacementEntry
        {
            public int Index { get; set; }
            public DatReader SourceDat { get; set; }
            public int SourceIndex { get; set; }
            public string SourceFile { get; set; }
        }

        class NewEntry
        {
            public DatEntry ArchEntry { get; set; } = new DatEntry();
            public DatEntry OrigEntry { get; set; }
            public int OrigIndex { get; set; }
            public ReplacementEntry ReplacementEntry { get; set; }
        }

        DatReader sourceDat;

        public List<ReplacementEntry> ReplacementEntries { get; } = new List<ReplacementEntry>();

        public DatBuilder(DatReader sourceDat = null)
        {
            this.sourceDat = sourceDat;
        }

        public void Build(Stream destStream)
        {
            // Check there are no duplicated indexes
            HashSet<int> indexSet = new HashSet<int>();
            foreach (var entry in ReplacementEntries)
            {
                if (entry.Index < 0) throw new InvalidOperationException("Entry with negative index present.");
                indexSet.Add(entry.Index);
            }
            if (indexSet.Count != ReplacementEntries.Count)
            {
                throw new InvalidOperationException("Replacement entries with non-unique IDs present.");
            }

            ReplacementEntries.Sort((x, y) => x.Index.CompareTo(y.Index));
            List<NewEntry> newEntries = new List<NewEntry>();

            // Copy over original entries
            if (sourceDat != null)
            {
                for (int i = 0; i < sourceDat.EntriesCount; ++i)
                {
                    var e = sourceDat.GetEntry(i);
                    newEntries.Add(new NewEntry { OrigEntry = e, OrigIndex = i });
                }
            }

            // Set replacement entries
            foreach (var rep in ReplacementEntries)
            {
                if (rep.Index > newEntries.Count)
                    throw new InvalidOperationException("Replacement entries results in incontinuity.");

                var newEntry = new NewEntry { ReplacementEntry = rep };
                if (rep.Index == newEntries.Count)
                    newEntries.Add(newEntry);
                else
                    newEntries[rep.Index] = newEntry;
            }

            // Update size and position
            uint dataOffset = 0;
            for (int i = 0; i < newEntries.Count; ++i)
            {
                var newEntry = newEntries[i];
                newEntry.ArchEntry.Offset = dataOffset;
                var repEntry = newEntry.ReplacementEntry;

                if (repEntry == null)
                {
                    newEntry.ArchEntry.Length = newEntry.OrigEntry.Length;
                }
                else
                {
                    if (repEntry.SourceDat != null)
                    {
                        if (repEntry.SourceFile != null)
                            throw new InvalidOperationException("Replacement entries with both DAT and file source specified exist.");
                        newEntry.ArchEntry.Length = repEntry.SourceDat.GetEntry(repEntry.SourceIndex).Length;
                    }
                    else if (repEntry.SourceFile != null)
                    {
                        newEntry.ArchEntry.Length = (uint)new FileInfo(repEntry.SourceFile).Length;
                    }
                    else
                    {
                        newEntry.ArchEntry.Length = 0;
                        newEntries.RemoveAt(i);
                        --i;
                    }
                }
                dataOffset += newEntry.ArchEntry.Length;
            }

            // Write file
            BinaryWriter bw = new BinaryWriter(destStream);
            bw.Write("DAT\0".ToCharArray());
            bw.Write(newEntries.Count);
            dataOffset = (uint)(((newEntries.Count + 1) * 8 + 15) & ~15);
            foreach (var newEntry in newEntries)
            {
                newEntry.ArchEntry.Offset += dataOffset;
                newEntry.ArchEntry.Write(bw);
            }
            // Do we need to 16-byte align anything after the header?
            if (destStream.Position < dataOffset)
                bw.Write(new byte[dataOffset - destStream.Position]);

            foreach (var newEntry in newEntries)
            {
                var repEntry = newEntry.ReplacementEntry;
                if (repEntry == null)
                {
                    bw.Write(sourceDat.GetData(newEntry.OrigIndex));
                }
                else
                {
                    if (repEntry.SourceDat != null)
                    {
                        bw.Write(repEntry.SourceDat.GetData(repEntry.SourceIndex));
                    }
                    else
                    {
                        using (FileStream fs = File.OpenRead(repEntry.SourceFile))
                        {
                            fs.CopyTo(destStream);
                        }
                    }
                }
            }
        }
    }
}
