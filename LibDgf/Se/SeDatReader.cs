using LibDgf.Dat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Se
{
    public class SeDatReader
    {
        DatReader dat;
        List<SeEntry> entries = new List<SeEntry>();
        byte[] seToc;
        byte[] seData;

        public SeDatReader(DatReader dat)
        {
            this.dat = dat ?? throw new ArgumentNullException(nameof(dat));

            seToc = dat.GetData(0);
            seData = dat.GetData(1);

            using (MemoryStream ms = new MemoryStream(seToc))
            {
                BinaryReader br = new BinaryReader(ms);
                while (true)
                {
                    SeEntry entry = new SeEntry();
                    entry.Read(br);
                    if (entry.DataOffset == -1 && entry.DataLength == -1)
                        break;
                    entries.Add(entry);
                }
            }
        }

        public int EntriesCount
        {
            get
            {
                return entries.Count;
            }
        }

        public byte[] GetData(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            if (index >= EntriesCount) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than or equal to count.");

            var entry = entries[index];
            byte[] data = new byte[entry.DataLength];
            Buffer.BlockCopy(seData, (int)entry.DataOffset, data, 0, data.Length);
            return data;
        }

        public string GetDataAsString(int index)
        {
            var data = GetData(index);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                if (b == 0) break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }
    }
}
