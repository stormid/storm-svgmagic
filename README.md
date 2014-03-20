storm-svgmagic
==============

Provides HttpHandler fallback functionality for browsers that do not support SVG (Scalable Vector Graphics) by generating and caching alternate images at request time.  Inspired by SVGMagic (http://svgmagic.bitlabs.nl/)

Demo
==============
http://svgmagic.azurewebsites.net

Use your browser tools to emulate IE 8 to see it in action, as storm.svgmagic allows the local browser cache to maintain cached copies you may have to do a hard refresh (ctrl-f5) to see changes

How to Use
==========
storm-svgmagic is available as a nuget package:

install-package storm-svgmagic

The nuget installer will modify your web.config to add the following:

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

to do

Per Image Configuration Overrides
=================================

to do