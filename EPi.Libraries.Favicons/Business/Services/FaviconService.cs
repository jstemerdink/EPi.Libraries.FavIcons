// Copyright © 2015 Jeroen Stemerdink. 
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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;

using EPi.Libraries.Favicons.Attributes;
using EPi.Libraries.Favicons.Models;

using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;

using ImageResizer;

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
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(FaviconService));

        /// <summary>
        ///     Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        private Injected<IContentRepository> ContentRepository { get; set; }

        /// <summary>
        ///     Gets or sets the synchronized object instance cache.
        /// </summary>
        /// <value>The synchronized object instance cache.</value>
        private Injected<ISynchronizedObjectInstanceCache> SynchronizedObjectInstanceCache { get; set; }

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
                        string.Format(CultureInfo.InvariantCulture, "{0}/{1}", iconsPath, "mstile-70x70.png"));

                // The URL to the 150x150 medium tile image.
                string square150X150LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "{0}/{1}", iconsPath, "mstile-150x150.png"));

                // The URL to the 310x310 large tile image.
                string square310X310LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "{0}/{1}", iconsPath, "mstile-310x310.png"));

                // The URL to the 310x150 wide tile image.
                string wide310X150LogoUrl =
                    urlHelper.Content(
                        string.Format(CultureInfo.InvariantCulture, "{0}/{1}", iconsPath, "mstile-310x150.png"));

                // The colour of the tile. This colour only shows if part of your images above are transparent.
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
                                "{0}/{1}",
                                iconsPath,
                                "android-chrome-36x36.png"),
                            "36x36",
                            "image/png",
                            "0.75"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}/{1}",
                                iconsPath,
                                "android-chrome-48x48.png"),
                            "48x48",
                            "image/png",
                            "1.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}/{1}",
                                iconsPath,
                                "android-chrome-72x72.png"),
                            "72x72",
                            "image/png",
                            "1.5"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}/{1}",
                                iconsPath,
                                "android-chrome-96x96.png"),
                            "96x96",
                            "image/png",
                            "2.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}/{1}",
                                iconsPath,
                                "android-chrome-144x144.png"),
                            "144x144",
                            "image/png",
                            "3.0"),
                        GetIconJObject(
                            urlHelper,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}/{1}",
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
                string siteName = Regex.Replace(SiteDefinition.Current.Name.ToLowerInvariant(), "[^a-z0-9\\-]", "");
                string iconRootPath = ConfigurationManager.AppSettings["sitesettings:iconroot"];

                if (string.IsNullOrWhiteSpace(iconRootPath))
                {
                    iconRootPath = "/content/icons/";
                }

                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", iconRootPath, siteName);
            }
            catch (Exception exception)
            {
                Logger.Warning("[Favicons] Error gettting the icon path", exception);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Gets the icon path.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetIconPath()
        {
            try
            {
                return
                    HostingEnvironment.MapPath(
                        string.Format(CultureInfo.InvariantCulture, "~{0}", this.GetVirtualIconPath()));
            }
            catch (Exception exception)
            {
                Logger.Warning("[Favicons] Error gettting the icon path", exception);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Checks if the icon path exists.
        /// </summary>
        /// <returns><c>true</c> if the icon path exists, <c>false</c> otherwise.</returns>
        public bool IconPathExists()
        {
            string iconsPath = this.GetIconPath();
            return this.IconPathExists(iconsPath);
        }

        /// <summary>
        ///     Checks if the icon path exists.
        /// </summary>
        /// <param name="iconsPath">The icons path.</param>
        /// <returns><c>true</c> if the icon path exists, <c>false</c> otherwise.</returns>
        public bool IconPathExists(string iconsPath)
        {
            return !string.IsNullOrWhiteSpace(iconsPath) && Directory.Exists(iconsPath);
        }

        /// <summary>
        ///     Gets the favicon settings.
        /// </summary>
        /// <returns>FaviconSettings.</returns>
        public FaviconSettings GetFaviconSettings()
        {
            const string FaviconCacheKey = "FaviconSettings";

            FaviconSettings faviconSettings =
                this.SynchronizedObjectInstanceCache.Service.Get(FaviconCacheKey) as FaviconSettings;

            if (faviconSettings != null)
            {
                return faviconSettings;
            }

            ContentData contentData;
            this.ContentRepository.Service.TryGet(SiteDefinition.Current.StartPage, out contentData);

            string applicationName = this.GetPropertyValue<ApplicationNameAttribute, string>(contentData);
            string applicationShortName = this.GetPropertyValue<ApplicationShortNameAttribute, string>(contentData);
            string themeColor = this.GetPropertyValue<ThemeColorAttribute, string>(contentData);
            string tileColor = this.GetPropertyValue<TileColorAttribute, string>(contentData);
            string faviconsPath = this.GetVirtualIconPath();
            ContentReference faviconReference = this.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);
            ContentReference mobileAppIconReference =
                    this.GetPropertyValue<MobileAppIconAttribute, ContentReference>(contentData);

            faviconSettings = new FaviconSettings
                                  {
                                      ThemeColor = !string.IsNullOrWhiteSpace(themeColor) ? themeColor : "#1E1E1E",
                                      TileColor = !string.IsNullOrWhiteSpace(tileColor) ? tileColor : "#1E1E1E",
                                      DisplayFavicons = !ContentReference.IsNullOrEmpty(faviconReference),
                                      FaviconsPath = faviconsPath,
                                      ApplicationName = string.IsNullOrWhiteSpace(applicationName) ? SiteDefinition.Current.Name : applicationName,
                                      ApplicationShortName = string.IsNullOrWhiteSpace(applicationShortName) ? SiteDefinition.Current.Name : applicationShortName,
                                      MobileWebAppCapable = !ContentReference.IsNullOrEmpty(mobileAppIconReference)
                                  };

            CacheEvictionPolicy cacheEvictionPolicy =
                new CacheEvictionPolicy(new[] { DataFactoryCache.PageCommonCacheKey(SiteDefinition.Current.StartPage) });

            this.SynchronizedObjectInstanceCache.Service.Insert(FaviconCacheKey, faviconSettings, cacheEvictionPolicy);

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
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateFavicons(ContentReference iconReference)
        {
            string filePath;

            if (!this.GetFilePath(iconReference, out filePath))
            {
                return false;
            }

            string iconsPath;

            if (!this.GetOrCreateIconPath(out iconsPath))
            {
                return false;
            }

            ImageBuilder imageBuilder = ImageBuilder.Current;

            CreateAppleIcon(filePath, imageBuilder, iconsPath, 57, 57);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 60, 60);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 72, 72);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 76, 76);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 114, 114);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 120, 120);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 144, 144);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 152, 152);
            CreateAppleIcon(filePath, imageBuilder, iconsPath, 180, 180);

            CreateMsTileIcon(filePath, imageBuilder, iconsPath, 70, 70);
            CreateMsTileIcon(filePath, imageBuilder, iconsPath, 150, 150);
            CreateMsTileIcon(filePath, imageBuilder, iconsPath, 310, 310);
            CreateMsTileIcon(filePath, imageBuilder, iconsPath, 310, 150);

            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 36, 36);
            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 48, 48);
            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 72, 72);
            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 96, 96);
            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 144, 144);
            CreateAndroidIcon(filePath, imageBuilder, iconsPath, 192, 192);

            CreateFavicon(filePath, imageBuilder, iconsPath, 16, 16);
            CreateFavicon(filePath, imageBuilder, iconsPath, 32, 32);
            CreateFavicon(filePath, imageBuilder, iconsPath, 96, 96);
            CreateFavicon(filePath, imageBuilder, iconsPath, 192, 192);

            return true;
        }

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public void CreateMobileAppicons(ContentReference iconReference)
        {
            string filePath;

            if (!this.GetFilePath(iconReference, out filePath))
            {
                return;
            }

            string iconsPath;

            if (!this.GetOrCreateIconPath(out iconsPath))
            {
                return;
            }

            ImageBuilder imageBuilder = ImageBuilder.Current;

            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 1536, 2008);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 1496, 2048);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 768, 1004);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 748, 1024);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 640, 1096);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 640, 920);
            CreateAppleStartupIcon(filePath, imageBuilder, iconsPath, 320, 460);
        }

        private bool GetOrCreateIconPath(out string iconsPath)
        {
            iconsPath = this.GetIconPath();

            if (string.IsNullOrWhiteSpace(iconsPath))
            {
                return false;
            }

            if (!Directory.Exists(iconsPath))
            {
                Directory.CreateDirectory(iconsPath);
            }

            return true;
        }

        private bool GetFilePath(ContentReference iconReference, out string filePath)
        {
            filePath = string.Empty;

            ImageData iconFile;

            this.ContentRepository.Service.TryGet(iconReference, out iconFile);

            if (iconFile == null)
            {
                Logger.Warning("[Favicons] Icon file not found.");
                return false;
            }

            FileBlob binaryData = iconFile.BinaryData as FileBlob;

            if (binaryData == null)
            {
                Logger.Warning("[Favicons] Icon is not a file.");
                return false;
            }

            filePath = binaryData.FilePath;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Warning("[Favicons] Icon file has no path.");
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        public void CleanUpFavicons()
        {
            string iconsPath = this.GetIconPath();

            if (!this.IconPathExists(iconsPath))
            {
                return;
            }

            Directory.Delete(iconsPath, true);
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

        private static void CreateAppleIcon(
            string filePath,
            ImageBuilder imageBuilder,
            string iconsPath,
            int width,
            int height)
        {
            imageBuilder.Build(
                filePath,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/apple-touch-icon-{1}x{2}.png",
                    iconsPath,
                    width,
                    height),
                new ResizeSettings(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "width={0}&height={1}&crop=auto&format=png",
                        width,
                        height)),
                false);
        }

        private static void CreateAppleStartupIcon(string filePath,
            ImageBuilder imageBuilder,
            string iconsPath,
            int width,
            int height)
        {
            imageBuilder.Build(
                filePath,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/apple-touch-startup-image-{1}x{2}.png",
                    iconsPath,
                    width,
                    height),
                new ResizeSettings(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "width={0}&height={1}&crop=auto&format=png",
                        width,
                        height)),
                false);
        }

        private static void CreateMsTileIcon(
            string filePath,
            ImageBuilder imageBuilder,
            string iconsPath,
            int width,
            int height)
        {
            imageBuilder.Build(
                filePath,
                string.Format(CultureInfo.InvariantCulture, "{0}/mstile-{1}x{2}.png", iconsPath, width, height),
                new ResizeSettings(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "width={0}&height={1}&crop=auto&format=png",
                        width,
                        height)),
                false);
        }

        private static void CreateAndroidIcon(
            string filePath,
            ImageBuilder imageBuilder,
            string iconsPath,
            int width,
            int height)
        {
            imageBuilder.Build(
                filePath,
                string.Format(CultureInfo.InvariantCulture, "{0}/android-chrome-{1}x{2}.png", iconsPath, width, height),
                new ResizeSettings(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "width={0}&height={1}&crop=auto&format=png",
                        width,
                        height)),
                false);
        }

        private static void CreateFavicon(
            string filePath,
            ImageBuilder imageBuilder,
            string iconsPath,
            int width,
            int height)
        {
            imageBuilder.Build(
                filePath,
                string.Format(CultureInfo.InvariantCulture, "{0}/favicon-{1}x{2}.png", iconsPath, width, height),
                new ResizeSettings(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "width={0}&height={1}&crop=auto&format=png",
                        width,
                        height)),
                false);
        }
    }
}
