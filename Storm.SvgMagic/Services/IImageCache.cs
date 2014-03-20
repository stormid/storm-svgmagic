using System.IO;

namespace Storm.SvgMagic.Services
{
    public interface IImageCache
    {
        Stream Get(string urlPath, SvgMagicOptions options);
        void Put(Stream stream, string urlPath, SvgMagicOptions options);
    }
}
