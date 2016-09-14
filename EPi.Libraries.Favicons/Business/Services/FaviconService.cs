// Copyright © 2016 Jeroen Stemerdink.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;

using EPi.Libraries.Favicons.Attributes;
using EPi.Libraries.Favicons.Models;

using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPi.Libraries.Favicons.Business.Services
{
    /// <summary>
    ///     Class FaviconService.
    /// </summary>
    [ServiceConfiguration(typeof(IFaviconService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FaviconService : IFaviconService
    {
        /// <summary>
        ///     The logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger();

        /// <summary>
        ///     Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        private Injected<IContentRepository> ContentRepository { get; set; }

        /// <summary>
        ///     Gets or sets the content type repository.
        /// </summary>
        /// <value>The content type repository.</value>
        private Injected<IContentTypeRepository> ContentTypeRepository { get; set; }

        /// <summary>
        ///     Gets or sets the content model usage.
        /// </summary>
        /// <value>The content model usage.</value>
        private Injected<IContentModelUsage> ContentModelUsage { get; set; }

        /// <summary>
        ///     Gets or sets the synchronized object instance cache.
        /// </summary>
        /// <value>The synchronized object instance cache.</value>
        private Injected<ISynchronizedObjectInstanceCache> SynchronizedObjectInstanceCache { get; set; }

        private Injected<IContentCacheKeyCreator> ContentCacheKeyCreator { get; set; }

        /// <summary>
        ///     Gets the browserconfig XML for the current site. This allows you to customize the tile, when a user pins
        ///     the site to their Windows 8/10 start screen. See http://www.buildmypinnedsite.com and
        ///     https://msdn.microsoft.com/en-us/library/dn320426%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <returns>The browserconfig XML for the current site.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        public string GetBrowserConfigXml(RequestContext requestContext)
        {
            try
            {
                UrlHelper urlHelper = new UrlHelper(requestContext);
                FaviconSettings faviconSettings = this.GetFaviconSettings();
                string iconsPath = this.GetVirtualIconPath();

                // The URL to the 70x70 small tile image.
                string square70X70LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", iconsPath, "mstile-70x70.png"));

                // The URL to the 150x150 medium tile image.
                string square150X150LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", iconsPath, "mstile-150x150.png"));

                // The URL to the 310x310 large tile image.
                string square310X310LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", iconsPath, "mstile-310x310.png"));

                // The URL to the 310x150 wide tile image.
                string wide310X150LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", iconsPath, "mstile-310x150.png"));

                // The color of the tile. This color only shows if part of your images above are transparent.
                string tileColour = faviconSettings.TileColor;

                XDocument document =
                    new XDocument(
                        new XElement(
                            "browserconfig",
                            new XElement(
                                "msapplication",
                                new XElement(
                                    "tile",
                                    new XElement("square70x70logo", new XAttribute("src", square70X70LogoUrl)),
                                    new XElement("square150x150logo", new XAttribute("src", square150X150LogoUrl)),
                                    new XElement("square310x310logo", new XAttribute("src", square310X310LogoUrl)),
                                    new XElement("wide310x150logo", new XAttribute("src", wide310X150LogoUrl)),
                                    new XElement("TileColor", tileColour)))));

                return document.ToString(SaveOptions.None);
            }
            catch (ArgumentNullException argumentNullException)
            {
                Logger.Error("[Favicons] Error creating browserconfig xml", argumentNullException);
            }
            catch (FormatException formatException)
            {
                Logger.Error("[Favicons] Error creating browserconfig xml", formatException);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Logger.Error("[Favicons] Error creating browserconfig xml", invalidOperationException);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Gets the manifest JSON for the current site. This allows you to customize the icon and other browser
        ///     settings for Chrome/Android and FireFox (FireFox support is coming). See https://w3c.github.io/manifest/
        ///     for the official W3C specification. See http://html5doctor.com/web-manifest-specification/ for more
        ///     information. See https://developer.chrome.com/multidevice/android/installtohomescreen for Chrome's
        ///     implementation.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <returns>The manifest JSON for the current site.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        public string GetManifestJson(RequestContext requestContext)
        {
            UrlHelper urlHelper = new UrlHelper(requestContext);
            FaviconSettings faviconSettings = this.GetFaviconSettings();
            string iconsPath = this.GetVirtualIconPath();

            JObject document = new JObject(
                new JProperty("short_name", faviconSettings.ApplicationShortName),
                new JProperty("name", faviconSettings.ApplicationShortName),
                new JProperty(
                    "icons",
                    new JArray(
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-36x36.png"),
                            "36x36",
                            "image/png",
                            "0.75"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-48x48.png"),
                            "48x48",
                            "image/png",
                            "1.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-72x72.png"),
                            "72x72",
                            "image/png",
                            "1.5"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-96x96.png"),
                            "96x96",
                            "image/png",
                            "2.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-144x144.png"),
                            "144x144",
                            "image/png",
                            "3.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/{0}/{1}",
                                iconsPath,
                                "android-chrome-192x192.png"),
                            "192x192",
                            "image/png",
                            "4.0"))));

            return document.ToString(Formatting.Indented);
        }

        /// <summary>
        ///     Gets the icon path.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetVirtualIconPath()
        {
            try
            {
                // As you apparently cannot get the url for a folder, just return the hard coded path.
                return !ContentReference.IsNullOrEmpty(SiteDefinition.Current.SiteAssetsRoot)
                           ? "siteassets/favicons"
                           : "globalassets/favicons";
            }
            catch (Exception exception)
            {
                Logger.Warning("[Favicons] Error getting the icon path", exception);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Gets the favicon settings.
        /// </summary>
        /// <returns>FaviconSettings.</returns>
        /// <exception cref="InvalidOperationException">
        ///     [Favicons] No settings defined. Use ContainsSettings attribute on your
        ///     ContentType .
        /// </exception>
        public FaviconSettings GetFaviconSettings()
        {
           FaviconSettings faviconSettings =
                this.SynchronizedObjectInstanceCache.Service.Get(FaviconSettings.FaviconCacheKey) as FaviconSettings;

            if (faviconSettings != null)
            {
                return faviconSettings;
            }

            IContent content;

            ContentType type =
                this.ContentTypeRepository.Service.List()
                    .FirstOrDefault(c => HasAttribute<ContainsSettingsAttribute>(c.ModelType));

            if (type == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            ContentUsage settingsUsage = this.ContentModelUsage.Service.ListContentOfContentType(type).FirstOrDefault();

            if (settingsUsage == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            this.ContentRepository.Service.TryGet(settingsUsage.ContentLink.ToReferenceWithoutVersion(), out content);


            faviconSettings = this.SetFaviconSettings(content as ContentData);

            return faviconSettings;
        }

        /// <summary>
        ///     Gets the favicon settings.
        /// </summary>
        /// <returns>FaviconSettings.</returns>
        /// <exception cref="InvalidOperationException">[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .</exception>
        public FaviconSettings SetFaviconSettings(ContentData contentData)
        {
            if (contentData == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            this.SynchronizedObjectInstanceCache.Service.Remove(FaviconSettings.FaviconCacheKey);

            string applicationName = this.GetPropertyValue<ApplicationNameAttribute, string>(contentData);
            string applicationShortName = this.GetPropertyValue<ApplicationShortNameAttribute, string>(contentData);
            string themeColor = this.GetPropertyValue<ThemeColorAttribute, string>(contentData);
            string tileColor = this.GetPropertyValue<TileColorAttribute, string>(contentData);
            string faviconsPath = this.GetVirtualIconPath();

            ContentReference faviconReference =
                this.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);
            ContentReference mobileAppIconReference =
                this.GetPropertyValue<MobileAppIconAttribute, ContentReference>(contentData);

            FaviconSettings faviconSettings = new FaviconSettings
                                                  {
                                                      ThemeColor =
                                                          !string.IsNullOrWhiteSpace(themeColor)
                                                              ? themeColor
                                                              : "#1E1E1E",
                                                      TileColor =
                                                          !string.IsNullOrWhiteSpace(tileColor) ? tileColor : "#1E1E1E",
                                                      DisplayFavicons =
                                                          !ContentReference.IsNullOrEmpty(faviconReference),
                                                      FaviconsPath = faviconsPath,
                                                      ApplicationName =
                                                          string.IsNullOrWhiteSpace(applicationName)
                                                              ? SiteDefinition.Current.Name
                                                              : applicationName,
                                                      ApplicationShortName =
                                                          string.IsNullOrWhiteSpace(applicationShortName)
                                                              ? SiteDefinition.Current.Name
                                                              : applicationShortName,
                                                      MobileWebAppCapable =
                                                          !ContentReference.IsNullOrEmpty(mobileAppIconReference)
                                                  };

            string cacheKey = this.ContentCacheKeyCreator.Service.CreateCommonCacheKey(SiteDefinition.Current.StartPage);

            CacheEvictionPolicy cacheEvictionPolicy =
                new CacheEvictionPolicy(new[] { cacheKey });

            this.SynchronizedObjectInstanceCache.Service.Insert(FaviconSettings.FaviconCacheKey, faviconSettings, cacheEvictionPolicy);

            return faviconSettings;
        }

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentData">The content data.</param>
        /// <returns>TO.</returns>
        public TO GetPropertyValue<T, TO>(ContentData contentData) where T : Attribute where TO : class
        {
            if (contentData == null)
            {
                return default(TO);
            }

            PropertyInfo propertyInfo =
                contentData.GetType().GetProperties().Where(this.HasAttribute<T>).FirstOrDefault();

            if (propertyInfo == null)
            {
                return default(TO);
            }

            return contentData.GetValue(propertyInfo.Name) as TO;
        }

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentReference">The content reference.</param>
        /// <returns>TO.</returns>
        public TO GetPropertyValue<T, TO>(ContentReference contentReference) where T : Attribute where TO : class
        {
            ContentData contentData;
            this.ContentRepository.Service.TryGet(contentReference, out contentData);

            return contentData == null ? default(TO) : this.GetPropertyValue<T, TO>(contentData);
        }

        /// <summary>
        ///     Determines whether the specified content data has settings.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        /// <returns><c>true</c> if the specified content data has settings; otherwise, <c>false</c>.</returns>
        public bool HasSettings(ContentData contentData)
        {
            return contentData != null && HasAttribute<ContainsSettingsAttribute>(contentData.GetType());
        }

        /// <summary>
        ///     Gets a <see cref="JObject" /> containing the specified image details.
        /// </summary>
        /// <param name="urlHelper">The url helper.</param>
        /// <param name="iconPath">The path to the icon image.</param>
        /// <param name="sizes">The size of the image in the format AxB.</param>
        /// <param name="type">The MIME type of the image.</param>
        /// <param name="density">The pixel density of the image.</param>
        /// <returns>A <see cref="JObject" /> containing the image details.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        private static JObject GetIconJObject(
            UrlHelper urlHelper,
            string iconPath,
            string sizes,
            string type,
            string density)
        {
            return new JObject(
                new JProperty("src", urlHelper.Content(iconPath)),
                new JProperty("sizes", sizes),
                new JProperty("type", type),
                new JProperty("density", density));
        }

        /// <summary>
        ///     Determines whether the specified self has attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo">The propertyInfo.</param>
        /// <returns><c>true</c> if the specified self has attribute; otherwise, <c>false</c>.</returns>
        private bool HasAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            T attr = default(T);

            try
            {
                attr = (T)Attribute.GetCustomAttribute(propertyInfo, typeof(T));
            }
            catch (Exception)
            {
            }

            return attr != null;
        }

        /// <summary>
        ///     Determines whether the specified member information has attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <returns><c>true</c> if the specified member information has attribute; otherwise, <c>false</c>.</returns>
        private static bool HasAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            T attr = default(T);

            try
            {
                attr = (T)Attribute.GetCustomAttribute(memberInfo, typeof(T));
            }
            catch (Exception)
            {
            }

            return attr != null;
        }
    }
}
