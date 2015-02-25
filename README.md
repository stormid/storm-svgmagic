storm-svgmagic
==============

Provides HttpHandler fallback functionality for browsers that do not support SVG (Scalable Vector Graphics) by generating and caching alternate images at request time.  Inspired by SVGMagic (http://svgmagic.bitlabs.nl/)

Demo
==============
http://storm-svgmagic.azurewebsites.net

Use your browser tools to emulate IE 8 to see it in action, as storm.svgmagic allows the local browser cache to maintain cached copies you may have to do a hard refresh (ctrl-f5) to see changes

How to Use
==========
storm-svgmagic is available as a nuget package:

`install-package Storm.SvgMagic`

The nuget installer will modify your `web.config` to add the following:

```xml
<configSections>
	<section name="SvgMagic" type="Storm.SvgMagic.SvgMagicHandlerConfigurationSection, Storm.SvgMagic"/>
</configSections>

<system.webServer>
	<handlers>
		<add name="SvgMagic" verb="GET" path="*.svg" type="Storm.SvgMagic.SvgMagicHandler, Storm.SvgMagic" />
	</handlers>
</system.webServer>

<SvgMagic defaultImageFormat="Png" imageStorageBasePath="~/App_Data/SvgMagic" testMode="false" />
```

Global Configuration
====================

```xml
<SvgMagic defaultImageFormat="Png|Gif|Jpeg|Bmp" imageStorageBasePath="<app relative or rooted path>" testMode="false|true" />
```

defaultImageFormat - Determines the conversion format to use by default (if excluded Png is used)
imageStorageBasePath - Either a app relative path (using ~/) or a fully rooted path, the application must have write permissions to this directory, if excluded ~/App_Data/SvgMagic is used)
testMode - If set to TRUE, then all svg images will be rendered using their fallback image type regardless of browser support

Per Image Configuration Overrides
=================================
Rendering can be overridden on a per image basis by appending parameters to the svg url:

Override default image format
-----------------------------
To override the default image format add the "format=" parameter to the url, see below:
```html
<img src="~/myimage.svg?format=png|gif|jpeg|bmp" alt="my image" />
```

Set size dimensions for fallback image
-----------------------------
By default the fallback image rendered will use the dimensions from the original .svg, if however you want to ensure that the fallback image is rendered at specific dimensions you can specify the `height=` and/or `width=` url parameters.

If only one of `height=` or `width=` is specified then the fallback image will maintain with a relative height or width to maintain the appropriate aspect ratio.

Both height and width are specified in pixels.
```html
<img src="~/myimage.svg?height=100" alt="my image" />
<img src="~/myimage.svg?width=100" alt="my image" />
<img src="~/myimage.svg?width=100&height=100" alt="my image" />
```

Override SVG support checking
-----------------------------
To force the fallback image to be generated/rendered add the "force=true" parameter to the url, see below:
```html
<img src="~/myimage.svg?force=true" alt="my image" />
```

Force refresh of cached image
-----------------------------
To force the cached image to be re-generated add the "refresh=true" parameter to the url, see below:
```html
<img src="~/myimage.svg?refresh=true" alt="my image" />
```
This is better used to individually refresh an image directly, combine with the "format=" parameter to refresh a specific fallback image type:

* http://storm-svgmagic.azurewebsites.net/Content/images/scotland.svg?force=true&refresh=true
* http://storm-svgmagic.azurewebsites.net/Content/images/scotland.svg?force=true&refresh=true&format=png

Android Browser detection
-------------------------
In order to detect android browsers (specifically those that do not support SVG, which is Android browser version 4.3 and below) the App_Browsers folder with a browsers definition file is required.  See the sample project for a copy of this android.browser file