using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using Storm.SvgMagic.Exceptions;
using Storm.SvgMagic.Services;
using Storm.SvgMagic.Services.Impl;
using Svg;

namespace Storm.SvgMagic
{
    public class SvgMagicHandler : IHttpHandler
    {
        private readonly SvgMagicHandlerConfigurationSection _config;
        private readonly string _version;
        private readonly IImageCache _imageCache;

        protected SvgMagicHandlerConfigurationSection Configuration
        {
            get { return _config; }
        }

        public SvgMagicHandler()
        {
            _config = ConfigurationManager.GetSection(SvgMagicHandlerConfigurationSection.ConfigSectionName) as SvgMagicHandlerConfigurationSection ?? new SvgMagicHandlerConfigurationSection();
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public SvgMagicHandler(IImageCache imageCache)
        {
            _imageCache = imageCache;
        }

        protected virtual Stream ConvertSvg(Stream svgInput, SvgMagicOptions options, bool failOnError = false)
        {
            if (svgInput == null || !svgInput.CanRead)
            {
                if(failOnError) throw new SvgMagicException("Svg input stream was null or could not be read", HttpStatusCode.NotFound);
                return null;
            }

            var svg = SvgDocument.Open<SvgDocument>(svgInput);
            if (svg == null)
            {
                if (failOnError) throw new SvgMagicException("Unable to open stream as Svg", HttpStatusCode.UnsupportedMediaType);
                return null;
            }

            if ((svg.Height.Type == SvgUnitType.Percentage) || (svg.Width.Type == SvgUnitType.Percentage))
            {
                if (failOnError) throw new SvgMagicException("Svg's with percentage based dimensions are not supported for fallback conversion", HttpStatusCode.UnsupportedMediaType);
                return null;
            }

            if (options.HasDimensions())
            {
                svg.Height = options.Height;
                svg.Width = options.Width;
            }
            else if (options.Height > 0)
            {
                var aspectRatio = svg.Height.Value/svg.Width.Value;
                svg.Height = options.Height;
                svg.Width = options.Height/aspectRatio;
            }
            else if (options.Width > 0)
            {
                var aspectRatio = svg.Width/svg.Height;
                svg.Width = options.Width;
                svg.Height = options.Width/aspectRatio;
            }
            else
            {
                options.Height = int.Parse(svg.Height.Value.ToString());
                options.Width = int.Parse(svg.Width.Value.ToString());
            }

            var outputStream = new MemoryStream();
            using (var bmp = svg.Draw())
            {
                
                Thread.Sleep(50);
                switch (options.Format)
                {
                    case SvgMagicImageFormat.Bmp:
                        bmp.Save(outputStream, ImageFormat.Bmp);
                        break;
                    case SvgMagicImageFormat.Png:
                        bmp.Save(outputStream, ImageFormat.Png);
                        break;
                    case SvgMagicImageFormat.Jpeg:
                        bmp.Save(outputStream, ImageFormat.Jpeg);
                        break;
                    case SvgMagicImageFormat.Gif:
                        bmp.Save(outputStream, ImageFormat.Gif);
                        break;
                    default:
                        throw new SvgMagicException(string.Format("Unknown image format specified - {0}", options.Format), HttpStatusCode.UnsupportedMediaType);
                }
            }
            return outputStream;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        protected virtual bool ResourceExists(string resourcePath)
        {
            return File.Exists(resourcePath);
        }

        protected virtual DateTime GetResourceUpdateDateTime(string resourcePath)
        {
            return File.GetLastWriteTime(resourcePath);
        }

        protected virtual IImageCache GetImageCache(HttpContextBase context)
        {
            return _imageCache ?? new FileSystemImageCache(context.Request.MapPath(_config.ImageStorageBasePath));
        }

        protected virtual Stream GetResourceStream(string resourcePath, int retryCounter = 0)
        {
            if (string.IsNullOrWhiteSpace(resourcePath) || !File.Exists(resourcePath)) return null;

            try
            {
                Thread.Sleep(50);
                return File.Open(resourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                if (retryCounter >= 5) return null;
                return GetResourceStream(resourcePath, retryCounter++);
            }
        }

        private bool ShouldInvalidateCachedItem(DateTime resourceDateTime, DateTime cacheItemDateTime)
        {
            // remove milliseconds from times
            resourceDateTime = resourceDateTime.AddTicks(-(resourceDateTime.Ticks % TimeSpan.TicksPerSecond));
            cacheItemDateTime = cacheItemDateTime.AddTicks(-(cacheItemDateTime.Ticks % TimeSpan.TicksPerSecond));

            return resourceDateTime > cacheItemDateTime;
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var startTime = DateTime.Now;
            try
            {
                ProcessRequestCore(context);
            }
            catch (SvgMagicException ex)
            {
                context.Response.StatusCode = (int) ex.StatusCode;
                context.Response.StatusDescription = ex.Message;
                Trace.TraceError(ex.ToString());
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = "An unexpected failure occurred while processing the request";
                Trace.TraceError(ex.ToString());                
            }
            finally
            {
                var elapsed = DateTime.Now - startTime;
                context.Response.AddHeader("X-SvgMagic-Version", _version);
                context.Response.AddHeader("X-SvgMagic-ProcessingTime", elapsed.TotalMilliseconds.ToString());
                context.Response.Flush();
                context.Response.End();                
            }
        }

        protected virtual void ProcessRequestCore(HttpContextBase context)
        {
            if (context.Request.CurrentExecutionFilePathExtension != string.Format(".{0}", _config.SvgExtension))
            {
                context.Response.StatusCode = 415;
                context.Response.StatusDescription = string.Format("Invalid resource type for handler, handler supports files with '{0}' extension only", _config.SvgExtension);
                return;
            }

            var urlPath = context.Request.CurrentExecutionFilePath;
            var resourcePath = context.Request.MapPath(urlPath);

            if (!ResourceExists(resourcePath))
            {
                context.Response.StatusCode = 404;
                return;
            }

            DateTime modifiedSince;
            DateTime resourceModifiedDate = GetResourceUpdateDateTime(resourcePath);
            resourceModifiedDate = resourceModifiedDate.AddTicks(-(resourceModifiedDate.Ticks % TimeSpan.TicksPerSecond));

            string ifModifiedSince = context.Request.Headers["If-Modified-Since"];
            if (!string.IsNullOrEmpty(ifModifiedSince) && ifModifiedSince.Length > 0 && DateTime.TryParse(ifModifiedSince, out modifiedSince))
            {
                if (resourceModifiedDate <= modifiedSince)
                {
                    context.Response.StatusCode = 304;
                    context.Response.Flush();
                    context.Response.End();
                    return;
                }
            }

            context.Response.AddFileDependency(resourcePath);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetETagFromFileDependencies();
            context.Response.Cache.SetLastModifiedFromFileDependencies();

            var options = SvgMagicOptions.Parse(context.Request.QueryString, Configuration);

            if (options.Force || NoSvgSupport(options, context.Request.Browser))
            {
                var imageCache = GetImageCache(context);

                using (var cachedFileStream = imageCache.Get(urlPath, options))
                {
                    if (cachedFileStream == null || ShouldInvalidateCachedItem(resourceModifiedDate, imageCache.GetCacheItemModifiedDateTime(urlPath, options)))
                    {
                        using (var svg = GetResourceStream(resourcePath))
                        {
                            using (var outputStream = ConvertSvg(svg, options, true))
                            {
                                if (outputStream != null)
                                {
                                    imageCache.Put(outputStream, urlPath, options);
                                    outputStream.CopyTo(context.Response.OutputStream);
                                    outputStream.Close();
                                }
                                else
                                {
                                    context.Response.StatusCode = 415;
                                    context.Response.StatusDescription = "Unable to convert Svg to " +options.MimeType;
                                }
                            }
                            svg.Close();
                        }
                    }
                    else
                    {
                        cachedFileStream.CopyTo(context.Response.OutputStream);
                        cachedFileStream.Close();
                    }
                }
                context.Response.ContentType = options.MimeType;
            }
            else
            {
                context.Response.ContentType = Configuration.SvgMimeType;
                context.Response.TransmitFile(resourcePath);
            }
        }

        protected virtual bool NoSvgSupport(SvgMagicOptions options, HttpBrowserCapabilitiesBase browser)
        {
            return browser.Browser == "IE" && browser.MajorVersion < 9;
        }

        public bool IsReusable { get { return true; } }
    }
}
