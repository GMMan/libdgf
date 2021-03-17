using LibDgf.Ps2.Vif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Mesh
{
    public class Pdb
    {
        VuVector[] boundingBox;

        public int BoundingBoxType { get; set; }
        public VuFloat BoundingBallSize { get; set; }
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
        public Tdb Specular { get; set; }
        public Tdb Diffuse { get; set; }
        public Tdb Metallic { get; set; }

        public void Read(BinaryReader br)
        {
            var startOffset = br.BaseStream.Position;
            BoundingBoxType = br.ReadInt32();
            BoundingBallSize = br.ReadPs2Float();
            uint boundingBoxOffset = br.ReadUInt32();
            uint boundingBoxLength = br.ReadUInt32();
            uint[] tdbOffsets = new uint[3];
            uint[] tdbLengths = new uint[tdbOffsets.Length];
            for (int i = 0; i < tdbOffsets.Length; ++i)
            {
                tdbOffsets[i] = br.ReadUInt32();
                tdbLengths[i] = br.ReadUInt32();
            }

            // Junk bytes 0xc8 to 0xcf here

            if (boundingBoxLength != 0)
            {
                br.BaseStream.Seek(startOffset + boundingBoxOffset, SeekOrigin.Begin);
                BoundingBox = br.ReadBoundingBox();
            }
            else
            {
                BoundingBox = null;
            }

            if (tdbLengths[0] != 0)
            {
                br.BaseStream.Seek(startOffset + tdbOffsets[0], SeekOrigin.Begin);
                Specular = new Tdb();
                Specular.Read(br);
            }
            else
            {
                Specular = null;
            }

            if (tdbLengths[1] != 0)
            {
                br.BaseStream.Seek(startOffset + tdbOffsets[1], SeekOrigin.Begin);
                Diffuse = new Tdb();
                Diffuse.Read(br);
            }
            else
            {
                Diffuse = null;
            }

            if (tdbLengths[2] != 0)
            {
                br.BaseStream.Seek(startOffset + tdbOffsets[2], SeekOrigin.Begin);
                Metallic = new Tdb();
                Metallic.Read(br);
            }
            else
            {
                Metallic = null;
            }
        }
    }
}
