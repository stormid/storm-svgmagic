using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storm.SvgMagic.Services
{
    public interface IFileSystem
    {
        bool Exists(string path);
        Stream Open(string path);
        void Save(Stream stream, string path, bool overwrite = true);
        IEnumerable<string> ReadAllLines(string path, Encoding encoding);
        string ReadAllText(string path, Encoding encoding);
        byte[] ReadAllBytes(string path);
        void Create(string path, bool asDirectory = true);
        DateTime GetModificationDateTime(string path);
    }
}