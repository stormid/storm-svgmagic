using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storm.SvgMagic.Services.Impl
{
    public class LocalFileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return DirectoryExists(path) || FileExists(path);
        }

        private static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        private static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        private static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public Stream Open(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public void Save(Stream stream, string path, bool overwrite = true)
        {
            if(!overwrite && Exists(path)) throw new IOException("destination file exists");
            
            using (var writeStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.CopyTo(writeStream);
                writeStream.Flush();
                writeStream.Close();
            }
        }

        public IEnumerable<string> ReadAllLines(string path, Encoding encoding)
        {
            return File.ReadAllLines(path, encoding);
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(path, encoding);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void Create(string path, bool asDirectory = true)
        {
            if (asDirectory)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                using (var fs = File.Create(path))
                {
                    fs.Flush();
                }
            }
        }
    }
}