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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

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
        ///     Gets or sets the content type repository.
        /// </summary>
        /// <value>The content type repository.</value>
        private Injected<IContentTypeRepository> ContentTypeRepository { get; set; }

        /// <summary>
        /// Gets or sets the content model usage.
        /// </summary>
        /// <value>The content model usage.</value>
        private Injected<IContentModelUsage> ContentModelUsage { get; set; }

        /// <summary>
        ///     Gets or sets the content media resolver.
        /// </summary>
        /// <value>The content media resolver.</value>
        private Injected<ContentMediaResolver> ContentMediaResolver { get; set; }

        /// <summary>
        ///     Gets or sets the BLOB factory.
        /// </summary>
        /// <value>The BLOB factory.</value>
        private Injected<BlobFactory> BlobFactory { get; set; }

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

                ////ContentReference iconsFolder = this.GetOrCreateFaviconsFolder();
                ////ImageData image = this.ContentRepository.Service.GetChildren<ImageData>(iconsFolder).FirstOrDefault();

                ////if (image == null)
                ////{
                ////    return string.Empty;
                ////}

                ////Url internalUrl = this.UrlResolver.Service.GetUrl(image.ContentLink);

                ////UrlBuilder url = new UrlBuilder(internalUrl);
                ////string[] segments = url.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                ////return string.Join("/", segments.Length == 3 ? segments.Take(2) : segments.Skip(3).Take(2));
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
        /// <exception cref="InvalidOperationException">[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .</exception>
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
            

            ContentType type = this.ContentTypeRepository.Service.List().FirstOrDefault(c => HasAttribute<ContainsSettingsAttribute>(c.ModelType));
            
            if (type == null)
            {
                throw new InvalidOperationException("[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            ContentUsage settingsUsage = this.ContentModelUsage.Service.ListContentOfContentType(type).FirstOrDefault();

            if (settingsUsage == null)
            {
                throw new InvalidOperationException("[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            this.ContentRepository.Service.TryGet(settingsUsage.ContentLink, out contentData);

            string applicationName = this.GetPropertyValue<ApplicationNameAttribute, string>(contentData);
            string applicationShortName = this.GetPropertyValue<ApplicationShortNameAttribute, string>(contentData);
            string themeColor = this.GetPropertyValue<ThemeColorAttribute, string>(contentData);
            string tileColor = this.GetPropertyValue<TileColorAttribute, string>(contentData);
            string faviconsPath = this.GetVirtualIconPath();
            ContentReference faviconReference =
                this.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);
            ContentReference mobileAppIconReference =
                this.GetPropertyValue<MobileAppIconAttribute, ContentReference>(contentData);

            faviconSettings = new FaviconSettings
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
        /// Determines whether the specified content data has settings.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        /// <returns><c>true</c> if the specified content data has settings; otherwise, <c>false</c>.</returns>
        public bool HasSettings(ContentData contentData)
        {
            return contentData != null && HasAttribute<ContainsSettingsAttribute>(contentData.GetType());
        }

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateFavicons(ContentReference iconReference)
        {
            if (ContentReference.IsNullOrEmpty(iconReference))
            {
                return false;
            }

            ContentReference rootfolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(rootfolder))
            {
                return false;
            }

            ImageData faviconImageData;

            if (!this.ContentRepository.Service.TryGet(iconReference, out faviconImageData))
            {
                return false;
            }

            using (Stream s = faviconImageData.BinaryData.OpenRead())
            {
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 57, 57);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 60, 60);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 72, 72);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 76, 76);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 114, 114);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 120, 120);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 144, 144);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 152, 152);
                this.CreateFavicon(rootfolder, s, "apple-touch-icon", 180, 180);

                this.CreateFavicon(rootfolder, s, "mstile", 70, 70);
                this.CreateFavicon(rootfolder, s, "mstile", 150, 150);
                this.CreateFavicon(rootfolder, s, "mstile", 310, 310);
                this.CreateFavicon(rootfolder, s, "mstile", 310, 150);

                this.CreateFavicon(rootfolder, s, "android-chrome", 36, 36);
                this.CreateFavicon(rootfolder, s, "android-chrome", 48, 48);
                this.CreateFavicon(rootfolder, s, "android-chrome", 72, 72);
                this.CreateFavicon(rootfolder, s, "android-chrome", 96, 96);
                this.CreateFavicon(rootfolder, s, "android-chrome", 144, 144);
                this.CreateFavicon(rootfolder, s, "android-chrome", 192, 192);

                this.CreateFavicon(rootfolder, s, "favicon", 16, 16);
                this.CreateFavicon(rootfolder, s, "favicon", 32, 32);
                this.CreateFavicon(rootfolder, s, "favicon", 96, 96);
                this.CreateFavicon(rootfolder, s, "favicon", 192, 192);
            }

            return true;
        }

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public void CreateMobileAppicons(ContentReference iconReference)
        {
            if (ContentReference.IsNullOrEmpty(iconReference))
            {
                return;
            }

            ContentReference rootfolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(rootfolder))
            {
                return;
            }

            ImageData faviconImageData;

            if (!this.ContentRepository.Service.TryGet(iconReference, out faviconImageData))
            {
                return;
            }

            using (Stream s = faviconImageData.BinaryData.OpenRead())
            {
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 1536, 2008);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 1496, 2048);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 768, 1004);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 748, 1024);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 640, 1096);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 640, 1096);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 640, 920);
                this.CreateFavicon(rootfolder, s, "apple-touch-startup-image", 320, 460);
            }
        }

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        public void CleanUpFavicons()
        {
            ContentReference faviconsFolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(faviconsFolder))
            {
                return;
            }

            this.ContentRepository.Service.DeleteChildren(faviconsFolder, true, AccessLevel.NoAccess);
        }

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        public void DeleteFavicons()
        {
            ContentReference faviconsFolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(faviconsFolder))
            {
                return;
            }

            this.ContentRepository.Service.Delete(faviconsFolder, true);
        }

        /// <summary>
        ///     Gets the or create favicons folder.
        /// </summary>
        /// <returns>ContentReference.</returns>
        private ContentReference GetOrCreateFaviconsFolder()
        {
            ContentReference rootFolder = GetAssetsRootFolder();

            ContentFolder faviconsFolder = this.GetOrCreateFolder(rootFolder, "Favicons");

            return faviconsFolder == null ? ContentReference.EmptyReference : faviconsFolder.ContentLink;
        }

        private static ContentReference GetAssetsRootFolder()
        {
            ContentReference rootFolder = SiteDefinition.Current.SiteAssetsRoot;

            if (ContentReference.IsNullOrEmpty(rootFolder))
            {
                rootFolder = SiteDefinition.Current.GlobalAssetsRoot;
            }

            return rootFolder;
        }

        /// <summary>
        ///     Returns a <c>ContentFolder</c> folder
        /// </summary>
        /// <param name="parentFolder">The folder container.</param>
        /// <param name="folderName">Identifier for folder.</param>
        /// <returns>Stored <c>ContentFolder</c> folder; otherwise created folder.</returns>
        private ContentFolder GetOrCreateFolder(ContentReference parentFolder, string folderName)
        {
            ContentFolder storedFolder =
                this.ContentRepository.Service.GetChildren<ContentFolder>(parentFolder)
                    .FirstOrDefault(f => string.Compare(f.Name, folderName, StringComparison.OrdinalIgnoreCase) == 0);

            if (storedFolder != null)
            {
                return storedFolder;
            }

            ContentFolder parent;

            if (!this.ContentRepository.Service.TryGet(parentFolder, out parent))
            {
                return null;
            }

            try
            {
                ContentFolder folder = this.ContentRepository.Service.GetDefault<ContentFolder>(parent.ContentLink);
                folder.Name = folderName;

                ContentReference folderReference = this.ContentRepository.Service.Save(
                folder,
                SaveAction.Publish,
                AccessLevel.NoAccess);

                ContentFolder newFolder;

                return !this.ContentRepository.Service.TryGet(folderReference, out newFolder) ? null : newFolder;
            }
            catch (AccessDeniedException accessDeniedException)
            {
                Logger.Error("[Favicons] Error creating content folder.", accessDeniedException);
                return null;
            }
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
        /// Determines whether the specified member information has attribute.
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

        private void CreateFavicon(
            ContentReference rootFolder,
            Stream originalFile,
            string filePrefix,
            int width,
            int height)
        {
            //Get a suitable MediaData type from extension
            Type mediaType = this.ContentMediaResolver.Service.GetFirstMatching(".png");

            ContentType contentType = this.ContentTypeRepository.Service.Load(mediaType);

            try
            {
                //Get a new empty file data
                ImageData media = this.ContentRepository.Service.GetDefault<ImageData>(rootFolder, contentType.ID);

                media.Name = string.Format(CultureInfo.InvariantCulture, "{0}-{1}x{2}.png", filePrefix, width, height);

                //Create a blob in the binary container
                Blob blob = this.BlobFactory.Service.CreateBlob(media.BinaryDataContainer, ".png");

                ImageJob imageJob = new ImageJob(
                    originalFile,
                    blob.OpenWrite(),
                    new Instructions(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "width={0}&height={1}&crop=auto&format=png",
                            width,
                            height)))
                {
                    ResetSourceStream = true,
                    DisposeDestinationStream = true,
                    DisposeSourceObject = false
                };

                imageJob.Build();

                //Assign to file and publish changes
                media.BinaryData = blob;
                this.ContentRepository.Service.Save(media, SaveAction.Publish);
            }
            catch (AccessDeniedException accessDeniedException)
            {
                Logger.Error("[Favicons] Error creating icon.", accessDeniedException);
            }
            
        }
    }
}
