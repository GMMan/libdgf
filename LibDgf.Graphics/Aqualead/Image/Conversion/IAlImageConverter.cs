using LibDgf.Aqualead.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Aqualead.Image.Conversion
{
    public interface IAlImageConverter
    {
        string FileExtension { get; }
        bool HasAlternativeFile(AlImage image);
        bool CanConvert(string pixelFormat);
        void ConvertFromAl(AlImage image, Stream destStream);
        void ConvertFromAlAlt(AlImage image, Stream destStream);
    }
}
