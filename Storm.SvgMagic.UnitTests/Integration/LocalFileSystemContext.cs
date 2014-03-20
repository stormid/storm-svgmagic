using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Storm.SvgMagic.Services;
using Storm.SvgMagic.Services.Impl;
using Storm.SvgMagic.UnitTests.Base;

namespace Storm.SvgMagic.UnitTests.Integration
{
    [Category("Integration")]
    internal abstract class LocalFileSystemContext : ContextSpecification
    {
        protected IFileSystem _fileSystem;

        protected string _basePath;

        protected override void SharedContext()
        {
            _basePath = Path.GetTempPath();
            _fileSystem = new LocalFileSystem();
        }

        protected Stream GetSampleImageStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("Storm.SvgMagic.UnitTests.scotland.svg");
        }

        public class WhenSavingStream : LocalFileSystemContext
        {
            protected string _path;
            protected Stream _pathStream;

            protected override void Context()
            {
                _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _pathStream = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            }

            protected override void Because()
            {
                _fileSystem.Save(_pathStream, _path);
            }

            [Test]
            public void ShouldSaveStreamToDisk()
            {
                File.Exists(_path).ShouldBeTrue();
            }

            [Test]
            public void SavedStreamShouldContainExpectedContent()
            {
                File.ReadAllText(_path).ShouldEqual("hello world");
            }

            protected override void CleanUpContext()
            {
                File.Delete(_path);
            }
        }

        public class WhenSavingStreamToExistingFileWithOverwrite : LocalFileSystemContext
        {
            protected string _path;
            protected Stream _pathStream;

            protected override void Context()
            {
                _path = Path.GetTempFileName();
                _pathStream = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            }

            protected override void Because()
            {
                _fileSystem.Save(_pathStream, _path, true);
            }

            [Test]
            public void ShouldSaveStreamToDisk()
            {
                File.Exists(_path).ShouldBeTrue();
            }

            [Test]
            public void SavedStreamShouldContainExpectedContent()
            {
                File.ReadAllText(_path).ShouldEqual("hello world");
            }

            protected override void CleanUpContext()
            {
                File.Delete(_path);
            }
        }

        public class WhenOpeningFile : LocalFileSystemContext
        {
            protected string _path;
            protected Stream _result;

            protected override void Context()
            {
                _path = Path.GetTempFileName();
            }

            protected override void Because()
            {
                _result = _fileSystem.Open(_path);
            }

            [Test]
            public void ShouldHaveUseableStream()
            {
                _result.ShouldNotBeNull();
                _result.CanRead.ShouldBeTrue();

            }

            protected override void CleanUpContext()
            {
                _result.Dispose();
                File.Delete(_path);
            }
        }

        public class WhenSavingStreamToExistingFileWithoutOverwrite : LocalFileSystemContext
        {
            protected string _path;
            protected Stream _pathStream;

            private MethodThatThrows exception;

            protected override void Context()
            {
                _path = Path.GetTempFileName();
                _pathStream = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            }

            protected override void Because()
            {
                exception = () => _fileSystem.Save(_pathStream, _path, false);
            }

            [Test]
            public void ShouldThrowException()
            {
                typeof (IOException).ShouldBeThrownBy(exception);
            }

            protected override void CleanUpContext()
            {
                File.Delete(_path);
            }
        }

        public class WhenCreatingFile : LocalFileSystemContext
        {
            protected string _path;

            protected override void Context()
            {
                _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            protected override void Because()
            {
                _fileSystem.Create(_path, false);
            }

            [Test]
            public void ShouldExist()
            {
                File.Exists(_path).ShouldBeTrue();
            }

            protected override void CleanUpContext()
            {
                File.Delete(_path);
            }
        }

        public class WhenCreatingDirectory : LocalFileSystemContext
        {
            protected string _path;

            protected override void Context()
            {
                _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            protected override void Because()
            {
                _fileSystem.Create(_path);
            }

            [Test]
            public void ShouldExist()
            {
                Directory.Exists(_path).ShouldBeTrue();
            }
        }

        public class WhenCheckingThatDirectoryDoesNotExist : LocalFileSystemContext
        {
            protected string _path;
            protected bool _result;

            protected override void Context()
            {
                _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            protected override void Because()
            {
                _result = _fileSystem.Exists(_path);
            }

            [Test]
            public void ShouldExist()
            {
                _result.ShouldBeFalse();
            }
        }

        public class WhenCheckingThatDirectoryExists : LocalFileSystemContext
        {
            protected string _path;
            protected bool _result;

            protected override void Context()
            {
                _path = Path.GetTempPath();
            }

            protected override void Because()
            {
                _result = _fileSystem.Exists(_path);
            }

            [Test]
            public void ShouldExist()
            {
                _result.ShouldBeTrue();
            }
        }

        public class WhenCheckingThatFileDoesNotExist : LocalFileSystemContext
        {
            protected string _path;
            protected bool _result;

            protected override void Context()
            {
                _path = Path.GetRandomFileName();
            }

            protected override void Because()
            {
                _result = _fileSystem.Exists(_path);
            }

            [Test]
            public void ShouldExist()
            {
                _result.ShouldBeFalse();
            }
        }

        public class WhenCheckingThatFileExists : LocalFileSystemContext
        {
            protected string _path;
            protected bool _result;

            protected override void Context()
            {
                _path = Path.GetTempFileName();
            }

            protected override void Because()
            {
                _result = _fileSystem.Exists(_path);
            }

            [Test]
            public void ShouldExist()
            {
                _result.ShouldBeTrue();
            }

            protected override void CleanUpContext()
            {
                File.Delete(_path);
            }
        }
    }
}
