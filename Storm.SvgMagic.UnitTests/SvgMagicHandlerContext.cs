using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using NUnit.Framework;
using Storm.SvgMagic.Abstractions;
using Storm.SvgMagic.UnitTests.Base;
using System.Reflection;

namespace Storm.SvgMagic.UnitTests
{
    public class FakeSvgMagicHandler : SvgMagicHandler
    {
        private readonly IImageCache _imageCache;
        public FakeSvgMagicHandler(IImageCache imageCache)
        {
            _imageCache = imageCache;
        }

        public Func<string, DateTime> GetResourceUpdateDateTimeFunc { get; set; }
        protected override DateTime GetResourceUpdateDateTime(string resourcePath)
        {
            if (GetResourceUpdateDateTimeFunc != null)
            {
                return GetResourceUpdateDateTimeFunc(resourcePath);
            }
            return base.GetResourceUpdateDateTime(resourcePath);
        }

        public Func<string, bool> ResourceExistsFunc { get; set; }
        protected override bool ResourceExists(string resourcePath)
        {
            if (ResourceExistsFunc != null)
            {
                return ResourceExistsFunc(resourcePath);
            }
            return base.ResourceExists(resourcePath);
        }

        protected override IImageCache GetImageCache(HttpContextBase context)
        {
            return _imageCache;
        }

        public Func<string, bool, Stream> GetResourceStreamFunc { get; set; }
        protected override Stream GetResourceStream(string resourcePath, bool throwErrorOnFail = false)
        {
            if (GetResourceStreamFunc != null)
            {
                return GetResourceStreamFunc(resourcePath, throwErrorOnFail);
            }
            return base.GetResourceStream(resourcePath, throwErrorOnFail);
        }

        public Func<SvgMagicOptions, HttpBrowserCapabilitiesBase, bool> NoSvgSupportFunc { get; set; }
        protected override bool NoSvgSupport(SvgMagicOptions options, HttpBrowserCapabilitiesBase browser)
        {
            if (NoSvgSupportFunc != null)
            {
                return NoSvgSupportFunc(options, browser);
            }
            return base.NoSvgSupport(options, browser);
        }
    }

    public abstract class SvgMagicHandlerContext : ContextSpecification
    {
        protected FakeSvgMagicHandler _handler;
        protected Mock<HttpContextBase> _requestContext;
        protected Mock<HttpRequestBase> _request;
        protected Mock<HttpResponseBase> _response;
        protected NameValueCollection _headers;
        protected NameValueCollection _queryString;
        protected Mock<HttpCachePolicyBase> _cachePolicy;
        protected Stream _outputStream;
        protected SvgMagicOptions _options;

        protected Mock<HttpBrowserCapabilitiesBase> _browser;

        protected Mock<IImageCache> _imageCache;

        protected Stream GetSampleImageStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("Storm.SvgMagic.UnitTests.scotland.svg");
        }

        protected override void SharedContext()
        {
            _requestContext = CreateDependency<HttpContextBase>();
            _request = CreateDependency<HttpRequestBase>();
            _response = CreateDependency<HttpResponseBase>();
            _cachePolicy = CreateDependency<HttpCachePolicyBase>();
            _imageCache = CreateDependency<IImageCache>();
            _browser = CreateDependency<HttpBrowserCapabilitiesBase>();

            _headers = new NameValueCollection();
            _queryString = new NameValueCollection();
            _outputStream = new MemoryStream();

            _request.SetupGet(s => s.CurrentExecutionFilePath).Returns("/Content/images/scotland.svg");
            _request.SetupGet(s => s.CurrentExecutionFilePathExtension).Returns(".svg");
            _request.SetupGet(s => s.Headers).Returns(_headers);
            _request.SetupGet(s => s.QueryString).Returns(_queryString);
            _request.SetupGet(s => s.Browser).Returns(_browser.Object);

            _response.SetupAllProperties();
            _response.SetupGet(s => s.Cache).Returns(_cachePolicy.Object);
            _response.SetupGet(s => s.OutputStream).Returns(_outputStream);

            _requestContext.SetupGet(s => s.Request).Returns(_request.Object);
            _requestContext.SetupGet(s => s.Response).Returns(_response.Object);

            _handler = new FakeSvgMagicHandler(_imageCache.Object);
            
            _handler.ResourceExistsFunc = resourcePath => true;
            _handler.GetResourceStreamFunc = (s, e) => GetSampleImageStream();
        }

        protected override void Because()
        {
            _handler.ProcessRequest(_requestContext.Object);
        }

        public class WhenServingSvgToCompatibleBrowser : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/svg+xml");
            }

            [Test]
            public void ShouldNotHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Never);
            }

        }

        public class WhenServingSvgToInCompatibleBrowser : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _browser.SetupGet(s => s.Browser).Returns("IE");
                _browser.SetupGet(s => s.MajorVersion).Returns(8);

                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/png");
            }

            [Test]
            public void ShouldHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Once);
            }

        }

        public class WhenForcingSvgToPng : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _queryString.Add("force", "true");
                _queryString.Add("format", "png");

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/png");
            }

            [Test]
            public void ShouldHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Once);
            }

        }

        public class WhenForcingSvgToJpeg : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _queryString.Add("force", "true");
                _queryString.Add("format", "jpeg");

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/jpeg");
            }

            [Test]
            public void ShouldHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Once);
            }

        }

        public class WhenForcingSvgToGif : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _queryString.Add("force", "true");
                _queryString.Add("format", "gif");

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/gif");
            }

            [Test]
            public void ShouldHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Once);
            }

        }

        public class WhenForcingSvgToBmp : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _queryString.Add("force", "true");
                _queryString.Add("format", "bmp");

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
            }

            [Test]
            public void ShouldSendExpectedImageFormatToResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/bmp");
            }

            [Test]
            public void ShouldHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Once);
            }
        }

        public class WhenHandlerWronglyConfigured : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
                
                _request.SetupGet(s => s.CurrentExecutionFilePathExtension).Returns(".aspx");
            }

            [Test]
            public void ShouldSend500InternalServerError()
            {
                _response.Object.StatusCode.ShouldEqual(500);
            }
        }

        public class WhenResourceDoesNotExist : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());
                
                _handler.ResourceExistsFunc = s => false;
            }

            [Test]
            public void ShouldSend404HttpNotFound()
            {
                _response.Object.StatusCode.ShouldEqual(404);
            }

            [Test]
            public void ShouldNotHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Never);
            }
        }

        public class WhenImageCanBeServedFromBrowserCache : SvgMagicHandlerContext
        {
            private DateTime _browserCacheDate;
            private DateTime _resourceDate;

            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());

                _browserCacheDate = DateTime.Now;
                _resourceDate = _browserCacheDate.AddSeconds(-1);

                _headers.Add("If-Modified-Since", _browserCacheDate.ToString("R"));
                _handler.GetResourceUpdateDateTimeFunc = s => _resourceDate;
            }

            [Test]
            public void ShouldSend304NotModified()
            {
                _response.Object.StatusCode.ShouldEqual(304);
            }

            [Test]
            public void ShouldNotHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Never);
            }
        }

        public class WhenImageCanBeServedFromImageCacheForNonCompatibleBrowser : SvgMagicHandlerContext
        {
            protected override void Context()
            {
                _imageCache.Setup(s => s.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>())).Verifiable();

                _options = SvgMagicOptions.Parse(_queryString, new SvgMagicHandlerConfigurationSection());

                _imageCache.Setup(s => s.Get(It.IsAny<string>(), It.IsAny<SvgMagicOptions>()))
                    .Returns(GetSampleImageStream);
                _handler.NoSvgSupportFunc = (options, @base) => true;
            }

            [Test]
            public void ShouldSendImageCachedResponse()
            {
                _response.Object.ContentType.ShouldEqual("image/png");
            }

            [Test]
            public void ShouldNotHaveAlteredImageCache()
            {
                _imageCache.Verify(v => v.Put(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<SvgMagicOptions>()), Times.Never);
            }
        }
    }
}
