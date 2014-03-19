using System.Configuration;

namespace Storm.SvgMagic
{
    public class SvgMagicHandlerConfigurationSection : ConfigurationSection
    {
        public static string ConfigSectionName = "SvgMagic";

        [ConfigurationProperty("defaultImageFormat", DefaultValue = SvgMagicImageFormat.Png)]
        public SvgMagicImageFormat DefaultImageFormat
        {
            get { return (SvgMagicImageFormat)base["defaultImageFormat"]; }
            set { base["defaultImageFormat"] = value; }
        }

        [ConfigurationProperty("imageStorageBasePath", DefaultValue = "~/App_Data/SvgMagic")]
        public string ImageStorageBasePath
        {
            get { return base["imageStorageBasePath"].ToString(); }
            set
            {
                if (value.EndsWith("/")) value = value.Substring(0, value.Length - 1);
                base["defaultImageFormatimageStorageBasePath"] = value;
            }
        }

        [ConfigurationProperty("svgExtension", DefaultValue = "svg")]
        public string SvgExtension
        {
            get { return base["svgExtension"].ToString(); }
            set
            {
                if (value.StartsWith(".")) value = value.Substring(1, value.Length);
                base["svgExtension"] = value.ToLowerInvariant();
            }
        }

        [ConfigurationProperty("svgMimeType", DefaultValue = "image/svg+xml")]
        public string SvgMimeType
        {
            get { return base["svgMimeType"].ToString(); }
            set
            {
                base["svgMimeType"] = value.ToLowerInvariant();
            }
        }

        [ConfigurationProperty("testMode", DefaultValue = false)]
        public bool TestMode
        {
            get { return (bool)base["testMode"]; }
            set
            {
                base["svgMimeType"] = value;
            }
        }
    }
}