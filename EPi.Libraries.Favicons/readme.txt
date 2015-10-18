Add three properties to you start page and add the attributes shown below.

[WebsiteIcon]
[UIHint(UIHint.Image)]
public virtual ContentReference FavIcon { get; set; }

[ThemeColor]
public virtual string ThemeColor { get; set; }

[TileColor]
public virtual string TileColor { get; set; }

When you upload an image, all favicons will be created automatically.

Add the following to your header in your _Root,cshtml file to render the markup:

@{ Html.RenderPartial("Favicons");}

Don't forget to enable MVC Attribute routing. ( RouteTable.Routes.MapMvcAttributeRoutes(); )