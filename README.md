# EPi.Libraries.Favicons

By Jeroen Stemerdink

[![Build status](https://ci.appveyor.com/api/projects/status/cfp88oa4mh8h2fci?svg=true)](https://ci.appveyor.com/project/jstemerdink/epi-libraries-favicons)
[![GitHub version](https://badge.fury.io/gh/jstemerdink%2FEPi.Libraries.Favicons.svg)](http://badge.fury.io/gh/jstemerdink%2FEPi.Libraries.Favicons)
[![Platform](https://img.shields.io/badge/platform-.NET%206-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx)
[![Platform](https://img.shields.io/badge/platform-.NET%207-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx)
[![Platform](https://img.shields.io/badge/EPiServer-%2012-orange.svg?style=flat)](http://world.episerver.com/cms/)
[![NuGet](https://img.shields.io/badge/NuGet-Release-blue.svg)](http://nuget.episerver.com/en/OtherPages/Package/?packageId=EPi.Libraries.Favicons)
[![GitHub license](https://img.shields.io/badge/license-MIT%20license-blue.svg?style=flat)](license.txt)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jstemerdink%3AEPi.Libraries.Favicons&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jstemerdink%3AEPi.Libraries.Favicons)
## Instructions

Add the following attribute to the ContentType you use for your settings

```
[ContainsSettings]
```


Add properties to the ContentType you use for your settings and add the attributes shown below.

```
[WebsiteIcon]
[UIHint(UIHint.Image)]
public virtual ContentReference Favicon { get; set; }

[MobileAppIcon]
[UIHint(UIHint.Image)]
public virtual ContentReference AppIcon { get; set; } >> use if your site is webapp capable

[ThemeColor]
public virtual string ThemeColor { get; set; } >> defaults to "#1E1E1E"

[TileColor]
public virtual string TileColor { get; set; } >> defaults to "#1E1E1E"

[ApplicationName]
public virtual string ApplicationName { get; set; } >> defaults to the name in the site definition

[ApplicationShortName]
public virtual string ApplicationShortName { get; set; } >> defaults to the name in the site definition
```


Add [ImageResizer](EPi.Libraries.Favicons.ImageResizer/README.md) package to your solution

OR
 
Add [ImageProcessor](EPi.Libraries.Favicons.ImageProcessor/README.md) package to your solution

OR

Add [ImageProcessor](EPi.Libraries.Favicons.ImageSharp/README.md) package to your solution

OR

Create your own resizing service.

*You can create your own service by implementing IResizeService or ResizeServiceBase*

Add the following to your header to render the markup:

```
@{
    await Html.RenderPartialAsync("_Favicons");
}
```

Add MVC in your startup

## Parts

[A custom localization provider](EPi.Libraries.FavIcons/README.md)

[An Azure translation plugin for the localization provider](EPi.Libraries.FavIcons.ImageResizer/README.md)

### Thanks
to [Rehan Saeed](https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate) for some of the ideas.


> *Powered by ReSharper*

> [![image](https://i0.wp.com/jstemerdink.files.wordpress.com/2017/08/logo_resharper.png)](http://jetbrains.com)