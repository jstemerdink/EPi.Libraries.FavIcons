NOTE: If you are updating from version 1.2, please add the ContainsSettings attribute to it and republish the start page with the icons. 
They will be stored as Blobs from now on, which is much cleaner and better suited for Azure.

Add the following attribute to the ContentType you use for your settings

[ContainsSettings]


Add properties to the ContentType you use for your settings and add the attributes shown below.

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



Add the following to your header in your _Root.cshtml file to render the markup:

@{ Html.RenderPartial("Favicons");}



Don't forget to enable MVC Attribute routing. ( RouteTable.Routes.MapMvcAttributeRoutes(); )

