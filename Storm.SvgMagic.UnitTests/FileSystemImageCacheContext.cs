using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Storm.SvgMagic.Services;
using Storm.SvgMagic.Services.Impl;
using Storm.SvgMagic.UnitTests.Base;

namespace Storm.SvgMagic.UnitTests
{
    [Category("Image Cache")]
    internal class FileSystemImageCacheContext : ContextSpecification
    {
        protected IImageCache _imageCache;
        protected Mock<IFileSystem> _fileSystem;

        protected NameValueCollection _queryString;
        protected SvgMagicOptions _options;

        protected override void SharedContext()
        {
            _fileSystem = CreateDependency<IFileSystem>();

            _imageCache = new FileSystemImageCache(_fileSystem.Object);
            
            _queryString = new NameValueCollection();
            _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
        }

        public class WhenGettingNonExistingFileFromCache : FileSystemImageCacheContext
        {
            protected string _urlPath, _cacheUrlPath;
            protected Stream _result;

            protected override void Context()
            {
                _urlPath = "images/myimage.svg";
                _cacheUrlPath = "images/myimage.svg/myimage.png";
            }

            protected override void Because()
            {
                _result = _imageCache.Get(_urlPath, _options);
            }

            [Test]
            public void ShouldNotOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Open(It.Is<string>(u => u == _cacheUrlPath)), Times.Never);
            }
        }

        public class WhenGettingExistingFileFromCache : FileSystemImageCacheContext
        {
            protected string _existingUrlPath, _existingCacheUrlPath;
            protected Stream _result;

            protected override void Context()
            {
                _existingUrlPath = "images/myimage.svg";
                _existingCacheUrlPath = "images/myimage.svg/myimage.png";

                _fileSystem.Setup(s => s.Exists(_existingUrlPath)).Returns(true);
                _fileSystem.Setup(s => s.Exists(_existingCacheUrlPath)).Returns(true);
                _fileSystem.Setup(s => s.Open(It.Is<string>(u => u == _existingCacheUrlPath))).Returns(new MemoryStream()).Verifiable();
            }

            protected override void Because()
            {
                _result = _imageCache.Get(_existingUrlPath, _options);
            }

            [Test]
            public void ShouldOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Open(It.Is<string>(u => u == _existingCacheUrlPath)), Times.Once);
            }
        }

        public class WhenPuttingFileInCache : FileSystemImageCacheContext
        {
            protected string _urlPath, _cacheUrlPath;
            protected Stream _stream;

            protected override void Context()
            {
                _urlPath = "images/myimage.svg";
                _cacheUrlPath = "images/myimage.svg/myimage.png";

                _stream = new MemoryStream();

                _fileSystem.Setup(s => s.Exists(_urlPath)).Returns(true);
                _fileSystem.Setup(s => s.Exists(_cacheUrlPath)).Returns(false);
                _fileSystem.Setup(s => s.Save(It.Is<Stream>(u => u == _stream), It.Is<string>(u => u == _cacheUrlPath), true)).Verifiable();
            }

            protected override void Because()
            {
                _imageCache.Put(_stream, _urlPath, _options);
            }

            [Test]
            public void ShouldOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Save(It.Is<Stream>(u => u == _stream), It.Is<string>(u => u == _cacheUrlPath), true), Times.Once);
            }

            [Test]
            public void ShouldLeaveStreamInUseableState()
            {
                _stream.CanRead.ShouldBeTrue();
                _stream.Position.ShouldEqual(0);
            }

            protected override void Because_After()
            {
                _stream.Dispose();
            }
        }

        public class WhenPuttingFileInCacheWithDimensions : FileSystemImageCacheContext
        {
            protected string _urlPath, _cacheUrlPath;
            protected Stream _stream;

            protected override void Context()
            {
                _queryString = new NameValueCollection()
                {
                    { "height", "100"},
                    { "width", "100"}
                };

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());


                _urlPath = "images/myimage.svg";
                _cacheUrlPath = "images/myimage.svg/h100_w100_myimage.png";

                _stream = new MemoryStream();

                _fileSystem.Setup(s => s.Exists(_urlPath)).Returns(true);
                _fileSystem.Setup(s => s.Exists(_cacheUrlPath)).Returns(false);
                _fileSystem.Setup(s => s.Save(It.Is<Stream>(u => u == _stream), It.Is<string>(u => u == _cacheUrlPath), true)).Verifiable();
            }

            protected override void Because()
            {
                _imageCache.Put(_stream, _urlPath, _options);
            }

            [Test]
            public void ShouldOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Save(It.Is<Stream>(u => u == _stream), It.Is<string>(u => u == _cacheUrlPath), true), Times.Once);
            }

            [Test]
            public void ShouldLeaveStreamInUseableState()
            {
                _stream.CanRead.ShouldBeTrue();
                _stream.Position.ShouldEqual(0);
            }

            protected override void Because_After()
            {
                _stream.Dispose();
            }
        }

        public class WhenGettingNonExistingFileFromCacheWithDimensions : FileSystemImageCacheContext
        {
            protected string _urlPath, _cacheUrlPath;
            protected Stream _result;

            protected override void Context()
            {
                _queryString = new NameValueCollection()
                {
                    { "height", "100"},
                    { "width", "100"}
                };

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());

                _urlPath = "images/myimage.svg";
                _cacheUrlPath = "images/myimage.svg/myimage.png";
            }

            protected override void Because()
            {
                _result = _imageCache.Get(_urlPath, _options);
            }

            [Test]
            public void ShouldNotOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Open(It.Is<string>(u => u == _cacheUrlPath)), Times.Never);
            }
        }

        public class WhenGettingExistingFileFromCacheWithDimensions : FileSystemImageCacheContext
        {
            protected string _existingUrlPath, _existingCacheUrlPath;
            protected Stream _result;

            protected override void Context()
            {
                _queryString = new NameValueCollection()
                {
                    { "height", "100"},
                    { "width", "100"}
                };

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());

                _existingUrlPath = "images/myimage.svg";
                _existingCacheUrlPath = "images/myimage.svg/h100_w100_myimage.png";

                _fileSystem.Setup(s => s.Exists(_existingUrlPath)).Returns(true);
                _fileSystem.Setup(s => s.Exists(_existingCacheUrlPath)).Returns(true);
                _fileSystem.Setup(s => s.Open(It.Is<string>(u => u == _existingCacheUrlPath))).Returns(new MemoryStream()).Verifiable();
            }

            protected override void Because()
            {
                _result = _imageCache.Get(_existingUrlPath, _options);
            }

            [Test]
            public void ShouldOpenStreamToFile()
            {
                _fileSystem.Verify(v => v.Open(It.Is<string>(u => u == _existingCacheUrlPath)), Times.Once);
            }
        }
    }
}
