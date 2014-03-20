using System;
using System.IO;

namespace Storm.SvgMagic.Services
{
    public interface IImageCache
    {
        DateTime GetCacheItemModifiedDateTime(string urlPath, SvgMagicOptions options);
        Stream Get(string urlPath, SvgMagicOptions options);
        void Put(Stream stream, string urlPath, SvgMagicOptions options);
    }
}
