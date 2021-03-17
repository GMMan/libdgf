using LibDgf.Ps2.Vif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Mesh
{
    public class Tdb
    {
        VuVector[] boundingBox;

        public TdbFlags Flags { get; set; }
        public List<TdbTexture> Textures { get; } = new List<TdbTexture>();
        public VuVector[] BoundingBox
        {
            get
            {
                return boundingBox;
            }
            set
            {
                if (value != null && value.Length != 8) throw new ArgumentException("Wrong number of vertices for bounding box.", nameof(value));
                boundingBox = value;
            }
        }
        public Db2 Mesh { get; set; }

        public void Read(BinaryReader br)
        {
            var startPos = br.BaseStream.Position;
            Flags = (TdbFlags)br.ReadByte();
            Textures.Clear();
            byte numTextures = br.ReadByte();
            for (int i = 0; i < numTextures; ++i)
            {
                var tex = new TdbTexture();
                tex.Read(br);
                Textures.Add(tex);
            }
            var read = br.BaseStream.Position - startPos;
            var aligned = (read + 15) & ~15;
            br.BaseStream.Seek(aligned - read, SeekOrigin.Current);

            if ((Flags & TdbFlags.SkipBoundingBox) == 0)
            {
                BoundingBox = br.ReadBoundingBox();
            }

            Mesh = new Db2();
            Mesh.Read(br);
        }
    }
}
