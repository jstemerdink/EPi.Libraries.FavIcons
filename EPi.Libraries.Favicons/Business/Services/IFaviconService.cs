// Copyright © 2017 Jeroen Stemerdink. 
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
using System.Web.Routing;

using EPi.Libraries.Favicons.Models;

using EPiServer.Core;

namespace EPi.Libraries.Favicons.Business.Services
{
    /// <summary>
    ///     Interface IFaviconService
    /// </summary>
    public interface IFaviconService
    {
        /// <summary>
        ///     Gets the browserconfig XML for the current site. This allows you to customize the tile, when a user pins
        ///     the site to their Windows 8/10 start screen. See http://www.buildmypinnedsite.com and
        ///     https://msdn.microsoft.com/en-us/library/dn320426%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <returns>The browserconfig XML for the current site.</returns>
        /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
        string GetBrowserConfigXml(RequestContext requestContext);

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
        string GetManifestJson(RequestContext requestContext);

        /// <summary>
        ///     Gets the icon path.
        /// </summary>
        /// <returns>System.String.</returns>
        string GetVirtualIconPath();

        /// <summary>
        ///     Gets the favicon settings.
        /// </summary>
        /// <returns>FaviconSettings.</returns>
        FaviconSettings GetFaviconSettings();

        /// <summary>
        /// Sets the favicon settings.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        /// <returns>FaviconSettings.</returns>
        FaviconSettings SetFaviconSettings(ContentData contentData);

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentData">The content data.</param>
        /// <returns>TO.</returns>
        TO GetPropertyValue<T, TO>(ContentData contentData) where T : Attribute where TO : class;

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <typeparam name="TO">The type of the class to check.</typeparam>
        /// <param name="contentReference">The content reference.</param>
        /// <returns>TO.</returns>
        TO GetPropertyValue<T, TO>(ContentReference contentReference) where T : Attribute where TO : class;

        /// <summary>
        ///     Determines whether the specified content data has settings.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        /// <returns><c>true</c> if the specified content data has settings; otherwise, <c>false</c>.</returns>
        bool HasSettings(ContentData contentData);
    }
}
