# EPi.Libraries.FavIcons

By Jeroen Stemerdink

[![Build status](https://ci.appveyor.com/api/projects/status/cfp88oa4mh8h2fci?svg=true)](https://ci.appveyor.com/project/jstemerdink/epi-libraries-favicons)
[![GitHub version](https://badge.fury.io/gh/jstemerdink%2FEPi.Libraries.Favicons.svg)](http://badge.fury.io/gh/jstemerdink%2FEPi.Libraries.Favicons)
[![Platform](https://img.shields.io/badge/platform-.NET 4.5-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx)
[![Platform](https://img.shields.io/badge/EPiServer-%208.0.0-orange.svg?style=flat)](http://world.episerver.com/cms/)
[![NuGet](https://img.shields.io/badge/NuGet-Release-blue.svg)](http://nuget.episerver.com/en/OtherPages/Package/?packageId=EPi.Libraries.Favicons)
[![GitHub license](https://img.shields.io/badge/license-MIT%20license-blue.svg?style=flat)](LICENSE)

## Instructions

Add three properties to you start page and add the attributes shown below.

```
[WebsiteIcon]
[UIHint(UIHint.Image)]
public virtual ContentReference FavIcon { get; set; }

[ThemeColor]
public virtual string ThemeColor { get; set; }

[TileColor]
public virtual string TileColor { get; set; }
```

Add the following to your header in your _Root,cshtml file to render the markup:

```
@{ Html.RenderPartial("FavIcons");}
```