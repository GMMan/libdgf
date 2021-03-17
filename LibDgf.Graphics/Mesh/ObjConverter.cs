using LibDgf.Dat;
using LibDgf.Mesh;
using LibDgf.Ps2.Gs;
using LibDgf.Ps2.Gs.Registers;
using LibDgf.Txm;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibDgf.Graphics.Mesh
{
    public class ObjConverter : IDisposable
    {
        Dictionary<ulong, TxmHeader> textureCache = new Dictionary<ulong, TxmHeader>();
        DatReader textureDat;
        private bool disposedValue;
        int numWrittenTextures = 0;

        public ObjConverter(DatReader textureDat)
        {
            this.textureDat = textureDat;
        }

        public void ConvertObj(Pdb pdb, StreamWriter sw)
        {
            int startVert = 1;
            if (pdb.Specular != null)
            {
                sw.WriteLine("o specular");
                startVert = WriteObj(pdb.Specular, sw, startVert);
            }
            if (pdb.Diffuse != null)
            {
                sw.WriteLine("o diffuse");
                startVert = WriteObj(pdb.Diffuse, sw, startVert);
            }
            if (pdb.Metallic != null)
            {
                sw.WriteLine("o metallic");
                startVert = WriteObj(pdb.Metallic, sw, startVert);
            }
        }

        TxmHeader CreateTexture(Tdb tdb, int index, out ulong textureId)
        {
            var tdbTexture = tdb.Textures[index];
            textureId = ((ulong)tdbTexture.DatIndex << 32) | ((ulong)tdbTexture.ImageBufferBase << 16) | tdbTexture.ClutBufferBase;
            if (!textureCache.ContainsKey(textureId))
            {
                TxmHeader txm = new TxmHeader();
                txm.ImageBufferBase = tdbTexture.ImageBufferBase;
                txm.ClutBufferBase = tdbTexture.ClutBufferBase;
                textureCache.Add(textureId, txm);
                return txm;
            }
            return null;
        }

        void FillTxm(Db2Element elem, TxmHeader txm)
        {
            Tex0 tex0 = new Tex0 { Packed = BitConverter.ToUInt64(elem.GsRegs, 0x10) };
            txm.ImageSourcePixelFormat = (TxmPixelFormat)tex0.Psm;
            txm.ImageVideoPixelFormat = txm.ImageSourcePixelFormat;
            txm.ImageWidth = (short)(1 << tex0.Tw);
            txm.ImageHeight = (short)(1 << tex0.Th);
            txm.Misc = tex0.Tbw;
            if (txm.ImageSourcePixelFormat == TxmPixelFormat.PSMT4 || txm.ImageSourcePixelFormat == TxmPixelFormat.PSMT8)
            {
                txm.ClutPixelFormat = (TxmPixelFormat)tex0.Cpsm;
                if (txm.ImageSourcePixelFormat == TxmPixelFormat.PSMT4)
                {
                    txm.ClutWidth = 8;
                    txm.ClutHeight = 2;
                }
                else
                {
                    txm.ClutWidth = 16;
                    txm.ClutHeight = 16;
                }
            }
            else
            {
                txm.ClutPixelFormat = TxmPixelFormat.None;
            }
        }

        int WriteObj(Tdb tdb, StreamWriter sw, int startVert)
        {
            for (int i = 0; i < tdb.Mesh.Elements.Count; ++i)
            {
                var elem = tdb.Mesh.Elements[i];
                sw.WriteLine($"g elem_{i}");

                TxmHeader txm = CreateTexture(tdb, elem.TextureIndex, out var textureId);
                sw.WriteLine($"usemtl tex_{textureId:x12}");
                if (txm != null) FillTxm(elem, txm);

                // Write vertices
                foreach (var vert in elem.Vertices)
                {
                    sw.WriteLine($"v {(double)vert.X} {(double)vert.Y} {(double)vert.Z}");
                }

                foreach (var norm in elem.VertexNormals)
                {
                    sw.WriteLine($"vn {norm.Item1} {norm.Item2} {norm.Item3}");
                }

                foreach (var uv in elem.STCoordinates)
                {
                    sw.WriteLine($"vt {uv.Item1} {1 - uv.Item2}");
                }

                // Write faces
                int[] initVerts = new int[2];
                bool clockwise = true;
                if (elem.GifTagIndex == 3) // Triangle fans
                {
                    int initVertPos = 0;
                    for (int j = 0; j < elem.Vertices.Count; ++j)
                    {
                        if ((elem.Vertices[j].W.Packed & 0x00008000) != 0)
                        {
                            initVerts[initVertPos++] = startVert + j;
                        }
                        else
                        {
                            int currVert = startVert + j;
                            if (clockwise)
                            {
                                sw.WriteLine($"f {initVerts[0]}/{initVerts[0]}/{initVerts[0]} {initVerts[1]}/{initVerts[1]}/{initVerts[1]} {currVert}/{currVert}/{currVert}");
                            }
                            else
                            {
                                sw.WriteLine($"f {currVert}/{currVert}/{currVert} {initVerts[1]}/{initVerts[1]}/{initVerts[1]} {initVerts[0]}/{initVerts[0]}/{initVerts[0]}");
                            }
                            initVerts[1] = currVert;
                            initVertPos = 0;
                        }
                        clockwise = !clockwise;
                    }
                }
                else if (elem.GifTagIndex == 4) // Triangle strips
                {
                    for (int j = 0; j < elem.Vertices.Count; ++j)
                    {
                        int currVert = startVert + j;
                        if ((elem.Vertices[j].W.Packed & 0x00008000) == 0)
                        {
                            if (clockwise)
                            {
                                sw.WriteLine($"f {initVerts[0]}/{initVerts[0]}/{initVerts[0]} {initVerts[1]}/{initVerts[1]}/{initVerts[1]} {currVert}/{currVert}/{currVert}");
                            }
                            else
                            {
                                sw.WriteLine($"f {currVert}/{currVert}/{currVert} {initVerts[1]}/{initVerts[1]}/{initVerts[1]} {initVerts[0]}/{initVerts[0]}/{initVerts[0]}");
                            }
                        }
                        initVerts[0] = initVerts[1];
                        initVerts[1] = currVert;
                        clockwise = !clockwise;
                    }
                }
                else
                {
                    throw new NotSupportedException("Unknown face construction type");
                }
                startVert += elem.Vertices.Count;
                sw.WriteLine();
            }

            return startVert;
        }

        void CopyTexelsClut(BinaryReader br, BinaryWriter bw, TxmHeader pakTxm, TxmHeader textureTxm)
        {
            if (pakTxm.ClutPixelFormat != TxmPixelFormat.None)
                throw new ArgumentException("Cannot operate on source TXM with CLUT.", nameof(pakTxm));
            if (textureTxm.ClutPixelFormat == TxmPixelFormat.None) return;

            var destColumnParams = GsMemoryUtils.GetColumnParams(textureTxm.ClutPixelFormat);
            int copyLength = textureTxm.GetClutByteSize();
            int baseBlockNumber = textureTxm.ClutBufferBase - pakTxm.ImageBufferBase;
            int srcBase = 0x10 + pakTxm.GetClutByteSize();
            var destBase = 0x10;
            int bytesPerSrcLine = pakTxm.GetImageByteSize() / pakTxm.ImageHeight;
            int bytesPerDestLine = textureTxm.GetClutByteSize() / textureTxm.ClutHeight;

            bw.Write(new byte[copyLength]);
            int numXBlocks = textureTxm.ClutWidth / destColumnParams.Width;
            if (numXBlocks == 0) numXBlocks = 1;
            int numYBlocks = textureTxm.ClutHeight / (destColumnParams.Height * GsMemoryUtils.COLUMNS_PER_BLOCK);
            if (numYBlocks == 0) numYBlocks = 1;
            int destBlock = 0;
            for (int blockY = 0; blockY < numYBlocks; ++blockY)
            {
                for (int blockX = 0; blockX < numXBlocks; ++blockX)
                {
                    int blockNumber = baseBlockNumber + GsMemoryUtils.CalcBlockNumber(textureTxm.ClutPixelFormat, blockX, blockY, 1);
                    br.BaseStream.Seek(srcBase + GsMemoryUtils.CalcBlockMemoryOffset(pakTxm.ImageSourcePixelFormat, blockNumber),
                        SeekOrigin.Begin);
                    bw.BaseStream.Seek(destBase + GsMemoryUtils.CalcTxmImageOffset(destColumnParams, destBlock, textureTxm.ClutWidth),
                        SeekOrigin.Begin);
                    for (int i = 0; i < GsMemoryUtils.COLUMNS_PER_BLOCK; ++i)
                    {
                        byte[] col = GsMemoryUtils.ReadColumn(br, pakTxm.ImageSourcePixelFormat, bytesPerSrcLine);
                        GsMemoryUtils.WriteColumn(bw, textureTxm.ClutPixelFormat, bytesPerDestLine, col);
                    }
                    ++destBlock;
                }
            }

            // Dump palette
            //bw.BaseStream.Seek(destBase, SeekOrigin.Begin);
            //BinaryReader palBr = new BinaryReader(bw.BaseStream);
            //using (var palette = TxmConversion.ConvertTxmRgba32(palBr, textureTxm.ClutWidth, textureTxm.ClutHeight))
            //{
            //    palette.SaveAsPng($"palette_{numWrittenTextures}.png");
            //}
        }

        void CopyTexels(BinaryReader br, BinaryWriter bw, TxmHeader pakTxm, TxmHeader textureTxm)
        {
            if (pakTxm.ClutPixelFormat != TxmPixelFormat.None)
                throw new ArgumentException("Cannot operate on source TXM with CLUT.", nameof(pakTxm));

            var destColumnParams = GsMemoryUtils.GetColumnParams(textureTxm.ImageSourcePixelFormat);
            int copyLength = textureTxm.GetImageByteSize();
            int srcBase = 0x10 + pakTxm.GetClutByteSize();
            int baseBlockNumber = textureTxm.ImageBufferBase - pakTxm.ImageBufferBase;
            int destBase = 0x10 + textureTxm.GetClutByteSize();
            int bytesPerSrcLine = pakTxm.GetImageByteSize() / pakTxm.ImageHeight;
            int bytesPerDestLine = copyLength / textureTxm.ImageHeight;

            bw.Write(new byte[copyLength]);
            int numXBlocks = textureTxm.ImageWidth / destColumnParams.Width;
            if (numXBlocks == 0) numXBlocks = 1;
            int numYBlocks = textureTxm.ImageHeight / (destColumnParams.Height * GsMemoryUtils.COLUMNS_PER_BLOCK);
            if (numYBlocks == 0) numYBlocks = 1;
            int destBlock = 0;
            for (int blockY = 0; blockY < numYBlocks; ++blockY)
            {
                for (int blockX = 0; blockX < numXBlocks; ++blockX)
                {
                    int blockNumber = baseBlockNumber + GsMemoryUtils.CalcBlockNumber(textureTxm.ImageSourcePixelFormat, blockX, blockY, textureTxm.Misc);
                    br.BaseStream.Seek(srcBase + GsMemoryUtils.CalcBlockMemoryOffset(pakTxm.ImageSourcePixelFormat, blockNumber),
                        SeekOrigin.Begin);
                    bw.BaseStream.Seek(destBase + GsMemoryUtils.CalcTxmImageOffset(destColumnParams, destBlock, textureTxm.ImageWidth),
                        SeekOrigin.Begin);
                    for (int i = 0; i < GsMemoryUtils.COLUMNS_PER_BLOCK; ++i)
                    {
                        byte[] col = GsMemoryUtils.ReadColumn(br, pakTxm.ImageSourcePixelFormat, bytesPerSrcLine);
                        if (pakTxm.ImageSourcePixelFormat != textureTxm.ImageSourcePixelFormat)
                        {
                            col = PsmtMixer.MixColumn(col, pakTxm.ImageSourcePixelFormat, textureTxm.ImageSourcePixelFormat, i % 2 != 0);
                        }
                        GsMemoryUtils.WriteColumn(bw, textureTxm.ImageSourcePixelFormat, bytesPerDestLine, col);
                    }

                    ++destBlock;
                }
            }
        }

        public void ExportTextures(StreamWriter mtlWriter, string outputPath)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (textureDat == null) throw new InvalidOperationException("No texture pack supplied.");

            int i = 0;
            numWrittenTextures = 0;
            foreach (var pair in textureCache.OrderBy(p => p.Key))
            {
                string pngPath = $"{outputPath}{i}.png";
                string alphaPath = $"{outputPath}{i}_alpha.png";
                TxmHeader textureTxm = pair.Value;

                int txmIndex = (int)(pair.Key >> 32);
                using (var txmMs = new MemoryStream(textureDat.GetData(txmIndex)))
                {
                    BinaryReader txmBr = new BinaryReader(txmMs);
                    TxmHeader pakTxm = new TxmHeader();
                    pakTxm.Read(txmBr);

                    Image<Rgba32> img = null;
                    try
                    {
                        // Check if TXM is already suitable
                        if (/*pakTxm.ImageSourcePixelFormat == textureTxm.ImageSourcePixelFormat &&*/
                            pakTxm.ImageBufferBase == textureTxm.ImageBufferBase &&
                            pakTxm.ClutPixelFormat == textureTxm.ClutPixelFormat &&
                            pakTxm.ClutBufferBase == textureTxm.ClutBufferBase)
                        {
                            // Use TXM as-is
                            txmMs.Seek(0, SeekOrigin.Begin);
                            img = TxmConversion.ConvertTxmToImage(txmMs);

                            // Dump palette
                            //if (pakTxm.ClutPixelFormat != TxmPixelFormat.None)
                            //{
                            //    txmMs.Seek(0x10, SeekOrigin.Begin);
                            //    using (var palette = TxmConversion.ConvertTxmRgba32(txmBr, pakTxm.ClutWidth, pakTxm.ClutHeight))
                            //    {
                            //        palette.SaveAsPng($"palette_{numWrittenTextures}.png");
                            //    }
                            //}
                        }
                        else
                        {
                            // Generate new TXM
                            using (MemoryStream ms = new MemoryStream())
                            {
                                BinaryWriter bw = new BinaryWriter(ms);
                                textureTxm.Write(bw);
                                CopyTexelsClut(txmBr, bw, pakTxm, textureTxm);
                                CopyTexels(txmBr, bw, pakTxm, textureTxm);
                                bw.Flush();
                                ms.Seek(0, SeekOrigin.Begin);
                                img = TxmConversion.ConvertTxmToImage(ms);
                            }
                        }

                        // Save out color texture
                        using (var img24bpp = img.CloneAs<Rgb24>())
                        {
                            img24bpp.SaveAsPng(pngPath);
                        }

                        // Extract alpha channel as a separate image
                        using (var alphaImg = new Image<L8>(img.Width, img.Height))
                        {
                            for (int y = 0; y < alphaImg.Height; ++y)
                            {
                                var srcSpan = img.GetPixelRowSpan(y);
                                var destSpan = alphaImg.GetPixelRowSpan(y);
                                for (int x = 0; x < alphaImg.Width; ++x)
                                {
                                    var srcAlpha = srcSpan[x].A;
                                    destSpan[x] = new L8(srcAlpha);
                                }
                            }
                            alphaImg.SaveAsPng(alphaPath);
                        }
                    }
                    finally
                    {
                        if (img != null) img.Dispose();
                    }
                }

                mtlWriter.WriteLine($"newmtl tex_{pair.Key:x12}");
                mtlWriter.WriteLine("Kd 0.80000000 0.80000000 0.80000000");
                mtlWriter.WriteLine("Ka 0 0 0");
                mtlWriter.WriteLine("Ke 0 0 0");
                mtlWriter.WriteLine("Ks 0 0 0");
                mtlWriter.WriteLine("d 1");
                mtlWriter.WriteLine("illum 2");
                mtlWriter.WriteLine($"map_Kd {Path.GetFileName(pngPath)}");
                mtlWriter.WriteLine($"map_d {Path.GetFileName(alphaPath)}");
                mtlWriter.WriteLine();

                ++i;
                ++numWrittenTextures;
            }
        }

        public void Reset()
        {
            textureCache.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (textureDat != null) textureDat.Dispose();
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
