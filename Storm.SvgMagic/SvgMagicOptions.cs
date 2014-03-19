using System;
using System.Collections.Specialized;

namespace Storm.SvgMagic
{
    public class SvgMagicOptions
    {
        public static SvgMagicOptions Parse(NameValueCollection queryStringParams, SvgMagicHandlerConfigurationSection config)
        {
            var options = new SvgMagicOptions();

            var format = config.DefaultImageFormat;
            Enum.TryParse(queryStringParams.Get("format"), true, out format);
            options.Format = format;

            bool refresh;
            bool.TryParse(queryStringParams.Get("refresh"), out refresh);
            options.Refresh = refresh;

            bool force;
            bool.TryParse(queryStringParams.Get("force"), out force);
            options.Force = force;

            options.SetImageFormat(config);

            if (config.TestMode)
            {
                options.Force = true;
                options.Refresh = true;
            }

            return options;
        }

        private SvgMagicOptions()
        {
        }

        public SvgMagicImageFormat Format { get; private set; }
        public bool Refresh { get; private set; }
        public bool Force { get; private set; }

        public string MimeType { get; set; }
        public string Extension { get; set; }

        private void SetImageFormat(SvgMagicHandlerConfigurationSection config)
        {
            while (true)
            {
                switch (Format)
                {
                    case SvgMagicImageFormat.Gif:
                        MimeType = "image/gif";
                        Extension = "gif";
                        break;
                    case SvgMagicImageFormat.Png:
                        MimeType = "image/png";
                        Extension = "png";
                        break;
                    case SvgMagicImageFormat.Jpeg:
                        MimeType = "image/jpeg";
                        Extension = "jpg";
                        break;
                    case SvgMagicImageFormat.Bmp:
                        MimeType = "image/bmp";
                        Extension = "bmp";
                        break;
                    default:
                        Format = config.DefaultImageFormat;
                        continue;
                }
                break;
            }
        }
    }
}