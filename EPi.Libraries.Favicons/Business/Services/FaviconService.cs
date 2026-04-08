// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaviconService.cs" company="Jeroen Stemerdink">
//      Copyright © 2026 Jeroen Stemerdink.
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
//
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
//
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//      SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Nodes;
using EPiServer.Applications;
using Microsoft.Extensions.Logging;

namespace EPi.Libraries.Favicons.Business.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    using Attributes;
    using Models;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.Framework.Cache;
    using EPiServer.ServiceLocation;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;

    /// <summary>
    ///     Class FaviconService.
    /// </summary>
    [ServiceConfiguration(typeof(IFaviconService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FaviconService : IFaviconService
    {
        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<FaviconService> _logger;

        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly IContentModelUsage _contentModelUsage;
        private readonly IContentRepository _contentRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ISynchronizedObjectInstanceCache _synchronizedObjectInstanceCache;
        private readonly IApplicationResolver _applicationResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaviconService" /> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="contentModelUsage">The content model usage.</param>
        /// <param name="synchronizedObjectInstanceCache">The synchronized object instance cache.</param>
        /// <param name="contentCacheKeyCreator">The content cache key creator.</param>
        /// <param name="logger">The logger</param>
        /// <param name="applicationResolver">The application resolver</param>
        public FaviconService(
            IContentRepository contentRepository,
            IContentTypeRepository contentTypeRepository,
            IContentModelUsage contentModelUsage,
            ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache,
            IContentCacheKeyCreator contentCacheKeyCreator, 
            ILogger<FaviconService> logger, 
            IApplicationResolver applicationResolver)
        {
            this._contentRepository = contentRepository;
            this._contentTypeRepository = contentTypeRepository;
            this._contentModelUsage = contentModelUsage;
            this._synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
            this._contentCacheKeyCreator = contentCacheKeyCreator;
            this._logger = logger;
            _applicationResolver = applicationResolver;
        }

        /// <summary>
        ///     Gets the browser config XML for the current site. This allows you to customize the tile, when a user pins
        ///     the site to their Windows 8/10 start screen. See http://www.buildmypinnedsite.com and
        ///     https://msdn.microsoft.com/en-us/library/dn320426%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="actionContext">The request context.</param>
        /// <returns>The browser config XML for the current site.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        [CLSCompliant(false)]
        public string GetBrowserConfigXml(ActionContext actionContext)
        {
            try
            {
                UrlHelper urlHelper = new UrlHelper(actionContext: actionContext);
                FaviconSettings faviconSettings = this.GetFaviconSettings();
                string iconsPath = this.GetVirtualIconPath();

                // The URL to the 70x70 small tile image.
                string square70X70LogoUrl = urlHelper.Content(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        "/{0}/{1}",
                        arg0: iconsPath,
                        "mstile-70x70.png"));

                // The URL to the 150x150 medium tile image.
                string square150X150LogoUrl = urlHelper.Content(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        "/{0}/{1}",
                        arg0: iconsPath,
                        "mstile-150x150.png"));

                // The URL to the 310x310 large tile image.
                string square310X310LogoUrl = urlHelper.Content(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        "/{0}/{1}",
                        arg0: iconsPath,
                        "mstile-310x310.png"));

                // The URL to the 310x150 wide tile image.
                string wide310X150LogoUrl = urlHelper.Content(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        "/{0}/{1}",
                        arg0: iconsPath,
                        "mstile-310x150.png"));

                // The color of the tile. This color only shows if part of your images above are transparent.
                string tileColour = faviconSettings.TileColor;

                XDocument document = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(
                        "browserconfig",
                        new XElement(
                            "msapplication",
                            new XElement(
                                "tile",
                                new XElement("square70x70logo", new XAttribute("src", value: square70X70LogoUrl)),
                                new XElement("square150x150logo", new XAttribute("src", value: square150X150LogoUrl)),
                                new XElement("square310x310logo", new XAttribute("src", value: square310X310LogoUrl)),
                                new XElement("wide310x150logo", new XAttribute("src", value: wide310X150LogoUrl)),
                                new XElement("TileColor", content: tileColour)))));

                return document.ToString(options: SaveOptions.None);
            }
            catch (ArgumentNullException argumentNullException)
            {
                _logger.LogError(argumentNullException, "[Favicons] Error creating browser config xml");
            }
            catch (FormatException formatException)
            {
                _logger.LogError(formatException, "[Favicons] Error creating browser config xml");
            }
            catch (InvalidOperationException invalidOperationException)
            {
                _logger.LogError(invalidOperationException, "[Favicons] Error creating browser config xml");
            }

            return string.Empty;
        }

        /// <summary>Gets the favicon settings.</summary>
        /// <returns> An instance of the <see cref="FaviconSettings"/>. </returns>
        /// <exception cref="InvalidOperationException">[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .
        /// </exception>
        public FaviconSettings GetFaviconSettings()
        {
            if (this._synchronizedObjectInstanceCache.Get(key: FaviconSettings.FaviconCacheKey) is FaviconSettings faviconSettings)
            {
                return faviconSettings;
            }

            ContentType type = this._contentTypeRepository.List()
                .FirstOrDefault(c => HasAttribute<ContainsSettingsAttribute>(memberInfo: c.ModelType));

            if (type == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            ContentUsage settingsUsage =
                this._contentModelUsage.ListContentOfContentType(contentType: type).FirstOrDefault();

            if (settingsUsage == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            this._contentRepository.TryGet(settingsUsage.ContentLink.ToReferenceWithoutVersion(), content: out IContent content);

            faviconSettings = this.SetFaviconSettings(content as ContentData);

            return faviconSettings;
        }

        /// <summary>
        ///     Gets the manifest JSON for the current site. This allows you to customize the icon and other browser
        ///     settings for Chrome/Android and FireFox (FireFox support is coming). See https://w3c.github.io/manifest/
        ///     for the official W3C specification. See http://html5doctor.com/web-manifest-specification/ for more
        ///     information. See https://developer.chrome.com/multidevice/android/installtohomescreen for Chrome's
        ///     implementation.
        /// </summary>
        /// <param name="actionContext">The request context.</param>
        /// <returns>The manifest JSON for the current site.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        [CLSCompliant(false)]
        public string GetManifestJson(ActionContext actionContext)
        {
            UrlHelper urlHelper = new UrlHelper(actionContext: actionContext);
            FaviconSettings faviconSettings = this.GetFaviconSettings();
            string iconsPath = this.GetVirtualIconPath();

            JsonObject document =new JsonObject
            {
                ["short_name"] = faviconSettings.ApplicationShortName,
                ["name"] = faviconSettings.ApplicationShortName,
                ["icons"] = new JsonArray(
                    GetIconJObject(
                        urlHelper: urlHelper,
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-36x36.png"),
                        "36x36",
                        "image/png",
                        "0.75"),
                    GetIconJObject(
                        urlHelper: urlHelper, 
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-48x48.png"),
                        "48x48",
                        "image/png",
                        "1.0"),
                    GetIconJObject(
                        urlHelper: urlHelper,
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-72x72.png"),
                        "72x72",
                        "image/png",
                        "1.5"),
                    GetIconJObject(
                        urlHelper: urlHelper,
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-96x96.png"),
                        "96x96",
                        "image/png",
                        "2.0"),
                    GetIconJObject(
                        urlHelper: urlHelper,
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-144x144.png"),
                        "144x144",
                        "image/png",
                        "3.0"),
                    GetIconJObject(
                        urlHelper: urlHelper,
                        string.Format(provider: CultureInfo.InvariantCulture, "/{0}/{1}", arg0: iconsPath, "android-chrome-192x192.png"),
                        "192x192",
                        "image/png",
                        "4.0"))
            };

            return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentData">The content data.</param>
        /// <returns>The value of the property.</returns>
        public TO GetPropertyValue<T, TO>(ContentData contentData)
            where T : Attribute where TO : class
        {
            if (contentData == null)
            {
                return null;
            }

            PropertyInfo propertyInfo =
                contentData.GetType().GetProperties().FirstOrDefault(predicate: HasAttribute<T>);

            if (propertyInfo == null)
            {
                return null;
            }

            return contentData.GetValue(name: propertyInfo.Name) as TO;
        }

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentReference">The content reference.</param>
        /// <returns>The value of the property.</returns>
        public TO GetPropertyValue<T, TO>(ContentReference contentReference)
            where T : Attribute where TO : class
        {
            this._contentRepository.TryGet(contentLink: contentReference, content: out ContentData contentData);

            return contentData == null ? null : this.GetPropertyValue<T, TO>(contentData: contentData);
        }

        /// <summary>
        ///     Gets the icon path.
        /// </summary>
        /// <returns>The virtual icon path.</returns>
        public string GetVirtualIconPath()
        {
            try
            {
                Website website = _applicationResolver.GetByContext() as Website;

                // As you apparently cannot get the url for a folder, just return the hard coded path.
                return !ContentReference.IsNullOrEmpty(contentLink: website?.AssetsRoot)
                           ? "siteassets/favicons"
                           : "globalassets/favicons";
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "[Favicons] Error getting the icon path");
            }

            return string.Empty;
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

        /// <summary> Gets the favicon settings. </summary>
        /// <param name="contentData"> The content Data. </param>
        /// <returns> An instance of the <see cref="FaviconSettings"/>. </returns> 
        /// <exception cref="InvalidOperationException"> [Favicons] No settings defined. Use ContainsSettings attribute on your ContentType . </exception>
        public FaviconSettings SetFaviconSettings(ContentData contentData)
        {
            if (contentData == null)
            {
                throw new InvalidOperationException(
                    "[Favicons] No settings defined. Use ContainsSettings attribute on your ContentType .");
            }

            Application website = _applicationResolver.GetByContext();
            
            this._synchronizedObjectInstanceCache.Remove(key: FaviconSettings.FaviconCacheKey);

            string applicationName = this.GetPropertyValue<ApplicationNameAttribute, string>(contentData: contentData);
            string applicationShortName =
                this.GetPropertyValue<ApplicationShortNameAttribute, string>(contentData: contentData);
            string themeColor = this.GetPropertyValue<ThemeColorAttribute, string>(contentData: contentData);
            string tileColor = this.GetPropertyValue<TileColorAttribute, string>(contentData: contentData);
            string faviconsPath = this.GetVirtualIconPath();

            ContentReference faviconReference =
                this.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData: contentData);
            ContentReference mobileAppIconReference =
                this.GetPropertyValue<MobileAppIconAttribute, ContentReference>(contentData: contentData);

            FaviconSettings faviconSettings = new FaviconSettings
                                                  {
                                                      ThemeColor =
                                                          !string.IsNullOrWhiteSpace(value: themeColor)
                                                              ? themeColor
                                                              : "#1E1E1E",
                                                      TileColor =
                                                          !string.IsNullOrWhiteSpace(value: tileColor)
                                                              ? tileColor
                                                              : "#1E1E1E",
                                                      DisplayFavicons =
                                                          !ContentReference.IsNullOrEmpty(
                                                              contentLink: faviconReference),
                                                      FaviconsPath = faviconsPath,
                                                      ApplicationName =
                                                          string.IsNullOrWhiteSpace(value: applicationName)
                                                              ? website?.DisplayName
                                                              : applicationName,
                                                      ApplicationShortName =
                                                          string.IsNullOrWhiteSpace(value: applicationShortName)
                                                              ? website?.DisplayName
                                                              : applicationShortName,
                                                      MobileWebAppCapable =
                                                          !ContentReference.IsNullOrEmpty(
                                                              contentLink: mobileAppIconReference)
                                                  };

            string cacheKey =
                this._contentCacheKeyCreator.CreateCommonCacheKey(contentLink: ContentReference.StartPage);
            
            CacheEvictionPolicy cacheEvictionPolicy = new CacheEvictionPolicy([cacheKey]);

            this._synchronizedObjectInstanceCache.Insert(
                key: FaviconSettings.FaviconCacheKey,
                value: faviconSettings,
                evictionPolicy: cacheEvictionPolicy);

            return faviconSettings;
        }

        /// <summary>
        ///     Gets a <see cref="JsonObject" /> containing the specified image details.
        /// </summary>
        /// <param name="urlHelper">The url helper.</param>
        /// <param name="iconPath">The path to the icon image.</param>
        /// <param name="sizes">The size of the image in the format AxB.</param>
        /// <param name="type">The MIME type of the image.</param>
        /// <param name="density">The pixel density of the image.</param>
        /// <returns>A <see cref="JsonObject" /> containing the image details.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        private static JsonObject GetIconJObject(
            UrlHelper urlHelper,
            string iconPath,
            string sizes,
            string type,
            string density)
        {
            return new JsonObject
            {
                ["src"] = urlHelper.Content(contentPath: iconPath),
                ["sizes"] = sizes,
                ["type"] = type,
                ["density"] = density
            };
        }

        /// <summary>
        ///     Determines whether the specified self has attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="propertyInfo">The propertyInfo.</param>
        /// <returns><c>true</c> if the specified self has attribute; otherwise, <c>false</c>.</returns>
        private static bool HasAttribute<T>(PropertyInfo propertyInfo)
            where T : Attribute
        {
            T attr = null;

            try
            {
                attr = (T)Attribute.GetCustomAttribute(element: propertyInfo, typeof(T));
            }
            catch (Exception)
            {
                // Assume the property is not there when an exception is thrown
            }

            return attr != null;
        }

        /// <summary>
        ///     Determines whether the specified member information has attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <returns><c>true</c> if the specified member information has attribute; otherwise, <c>false</c>.</returns>
        private static bool HasAttribute<T>(MemberInfo memberInfo)
            where T : Attribute
        {
            T attr = null;

            try
            {
                attr = (T)Attribute.GetCustomAttribute(element: memberInfo, typeof(T));
            }
            catch (Exception)
            {
                // Assume the property is not there when an exception is thrown
            }

            return attr != null;
        }
    }
}