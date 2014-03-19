using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Storm.SvgMagic.Abstractions
{
    public interface IImageCache
    {
        Stream Get(string urlPath, SvgMagicOptions options);
        void Put(Stream stream, string urlPath, SvgMagicOptions options);
    }

    public interface IFileSystem
    {
    }

    public class FileSystemImageCache : IImageCache
    {
        private readonly string _basePath;

        public FileSystemImageCache(string basePath)
        {
            _basePath = basePath;
        }

        public Stream Get(string urlPath, SvgMagicOptions options)
        {
            var physicalPath = Path.Combine(_basePath, urlPath.TrimStart('/'));
            if (!Directory.Exists(physicalPath)) return null;

            var fileName = urlPath.Split('/').LastOrDefault();
            var extension = fileName.Split('.').LastOrDefault();

            var cachedFile = Path.Combine(physicalPath, fileName.Replace(extension, options.Extension));

            if (File.Exists(cachedFile))
            {
                return File.Open(cachedFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            return null;
        }

        public void Put(Stream stream, string urlPath, SvgMagicOptions options)
        {
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            if (stream.CanRead)
            {
                var physicalPath = Path.Combine(_basePath, urlPath.TrimStart('/'));
                if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

                var fileName = urlPath.Split('/').LastOrDefault();
                var extension = fileName.Split('.').LastOrDefault();
                var cachedFile = Path.Combine(physicalPath, fileName.Replace(extension, options.Extension));

                using (var fs = new FileStream(cachedFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
