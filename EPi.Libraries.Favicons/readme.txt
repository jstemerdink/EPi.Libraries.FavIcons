Add properties to you start page and add the attributes shown below.

[WebsiteIcon]
[UIHint(UIHint.Image)]
public virtual ContentReference Favicon { get; set; }

[ThemeColor]
public virtual string ThemeColor { get; set; } >> defaults to "#1E1E1E"

[TileColor]
public virtual string TileColor { get; set; } >> defaults to "#1E1E1E"

[ApplicationName]
public virtual string ApplicationName { get; set; } >> defaults to the name in the site definition

[ApplicationShortName]
public virtual string ApplicationShortName { get; set; } >> defaults to the name in the site definition



Add the following to your header in your _Root,cshtml file to render the markup:

@{ Html.RenderPartial("Favicons");}

Don't forget to enable MVC Attribute routing. ( RouteTable.Routes.MapMvcAttributeRoutes(); )