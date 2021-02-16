using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Archive
{
    public class AlAarEntryV2
    {
        uint rest;

        public uint Id { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
        public uint Range
        {
            get
            {
                return rest & 0xffffff;
            }
            set
            {
                if (value > 0xffffff) throw new ArgumentOutOfRangeException(nameof(value));
                rest = value | (rest & 0xff000000);
            }
        }
        public AlAarEntryFlags Flags
        {
            get
            {
                return (AlAarEntryFlags)((rest >> 24) & 0xff);
            }
            set
            {
                if ((uint)value > 0xff) throw new ArgumentOutOfRangeException(nameof(value));
                rest = ((uint)value << 24) | (rest & 0xffffff);
            }
        }
        public string Name { get; set; }

        public void Read(BinaryReader br)
        {
            Id = br.ReadUInt32();
            Offset = br.ReadUInt32();
            Length = br.ReadUInt32();
            rest = br.ReadUInt32();
        }
    }
}
