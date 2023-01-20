Add the following attribute to the ContentType you use for your settings

[ContainsSettings]


Add properties to the ContentType you use for your settings and add the attributes shown below.


        [Display(Name = "Favicon",
            Description = "Favicon to be used for the website",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 100)]
        [WebsiteIcon]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Favicon { get; set; }

        [Display(Name = "App icon",
            Description = "App icon to be used for the website, use if your site is webapp capable",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 110)]
        [MobileAppIcon]
        [UIHint(UIHint.Image)]
        public virtual ContentReference AppIcon { get; set; }

        [Display(Name = "Theme color",
            Description = "Theme color to be shown in Chrome, defaults to #1E1E1E",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 120)]
        [ThemeColor]
        public virtual string ThemeColor { get; set; }

        [Display(Name = "Tile color",
            Description = "Tile color to be shown in Windows 8, defaults to #1E1E1E",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 130)]
        [TileColor]
        public virtual string TileColor { get; set; }

        [Display(Name = "Application name",
            Description = "Application name for the website, defaults to the name in the site definition",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 140)]
        [ApplicationName]
        public virtual string ApplicationName { get; set; }

        [Display(Name = "Application shortname",
            Description = "Application short name for the website, defaults to the name in the site definition",
           GroupName = Global.GroupNames.SiteSettings,
           Order = 150)]
        [ApplicationShortName]
        public virtual string ApplicationShortName { get; set; }



Add the following to your header to render the markup:

@{
    await Html.RenderPartialAsync("_Favicons");
}



Don't forget to add MVC in your startup

NOTE: Please add EPi.Libraries.FavIcons.ImageResizer or EPi.Libraries.Favicons.ImageProcessor to your solution, or implement your own resizing service 

