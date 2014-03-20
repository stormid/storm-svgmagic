using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Storm.SvgMagic.Services.Impl
{
    public class FileSystemImageCache : IImageCache
    {
        private readonly string _basePath;
        private IFileSystem _fileSystem;

        public FileSystemImageCache(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _basePath = string.Empty;
        }

        public FileSystemImageCache(string basePath) : this(new LocalFileSystem())
        {
            _basePath = basePath;
        }

        public FileSystemImageCache() : this(new LocalFileSystem())
        {
            var config =
                ConfigurationManager.GetSection(SvgMagicHandlerConfigurationSection.ConfigSectionName) as SvgMagicHandlerConfigurationSection ?? new SvgMagicHandlerConfigurationSection();
            _basePath = config.ImageStorageBasePath;
        }

        private string GetCacheFilePathAndName(string urlPath, SvgMagicOptions options)
        {
            var physicalPath = Path.Combine(_basePath, urlPath.TrimStart('/')).Replace('\\', '/');
            if (!_fileSystem.Exists(physicalPath)) _fileSystem.Create(physicalPath);

            var fileName = urlPath.Split('/').LastOrDefault();
            fileName = BuildFileName(fileName, options);
            var extension = fileName.Split('.').LastOrDefault();

            var cachedFile = Path.Combine(physicalPath, fileName.Replace(extension, options.Extension));
            cachedFile = cachedFile.Replace('\\', '/');

            return cachedFile;
        }

        public DateTime GetCacheItemModifiedDateTime(string urlPath, SvgMagicOptions options)
        {
            var cachedFile = GetCacheFilePathAndName(urlPath, options);

            return _fileSystem.GetModificationDateTime(cachedFile);
        }

        public Stream Get(string urlPath, SvgMagicOptions options)
        {
            var cachedFile = GetCacheFilePathAndName(urlPath, options);

            if (_fileSystem.Exists(cachedFile))
            {
                return _fileSystem.Open(cachedFile);
            }

            return null;
        }

        private static string BuildFileName(string fileName, SvgMagicOptions options)
        {
            var cacheFileName = fileName;
            if (options.Width > 0)
            {
                cacheFileName = string.Format("w{0}_{1}", options.Width, cacheFileName);
            }

            if (options.Height > 0)
            {
                cacheFileName = string.Format("h{0}_{1}", options.Height, cacheFileName);
            }

            return cacheFileName;
        }

        public void Put(Stream stream, string urlPath, SvgMagicOptions options)
        {
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            if (stream.CanRead)
            {
                var cachedFile = GetCacheFilePathAndName(urlPath, options);

                _fileSystem.Save(stream, cachedFile);

                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}