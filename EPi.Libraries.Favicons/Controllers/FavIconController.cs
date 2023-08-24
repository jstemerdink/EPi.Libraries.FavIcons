// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaviconController.cs" company="Jeroen Stemerdink">
//      Copyright © 2023 Jeroen Stemerdink.
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

namespace EPi.Libraries.Favicons.Controllers
{
    using System;
    using System.Text;

    using Attributes;
    using Business.Services;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    ///     Class FaviconController.
    /// </summary>
    [CLSCompliant(false)]
    public class FaviconController : Controller
    {
        /// <summary>
        ///     Gets or sets the favicon service.
        /// </summary>
        /// <value>The favicon service.</value>
        private readonly IFaviconService faviconService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaviconController"/> class.
        /// </summary>
        /// <param name="faviconService">The favicon service.</param>
        public FaviconController(IFaviconService faviconService)
        {
            this.faviconService = faviconService;
        }

        /// <summary>
        ///     Gets the browserconfig XML for the current site. This allows you to customize the tile, when a user pins
        ///     the site to their Windows 8/10 start screen. See http://www.buildmypinnedsite.com and
        ///     https://msdn.microsoft.com/en-us/library/dn320426%28v=vs.85%29.aspx
        /// </summary>
        /// <returns>The browserconfig XML for the current site.</returns>
        [NoTrailingSlash]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        [Route("browserconfig.xml", Name = "GetBrowserConfigXml")]
        public ContentResult BrowserConfigXml()
        {
            string content = this.faviconService.GetBrowserConfigXml(actionContext: this.ControllerContext);
            return this.Content(content: content, @"application/xml", contentEncoding: Encoding.UTF8);
        }

        /// <summary>
        ///     Gets the manifest JSON for the current site. This allows you to customize the icon and other browser
        ///     settings for Chrome/Android and FireFox (FireFox support is coming). See https://w3c.github.io/manifest/
        ///     for the official W3C specification. See http://html5doctor.com/web-manifest-specification/ for more
        ///     information. See https://developer.chrome.com/multidevice/android/installtohomescreen for Chrome's
        ///     implementation.
        /// </summary>
        /// <returns>The manifest JSON for the current site.</returns>
        [NoTrailingSlash]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        [Route("manifest.json", Name = "GetManifestJson")]
        public ContentResult ManifestJson()
        {
            string content = this.faviconService.GetManifestJson(actionContext: this.ControllerContext);
            return this.Content(content: content, @"application/json", contentEncoding: Encoding.UTF8);
        }
    }
}