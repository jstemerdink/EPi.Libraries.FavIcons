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

using EPi.Libraries.Favicons.Attributes;
using EPi.Libraries.Favicons.Business.Services;

using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

using InitializationModule = EPiServer.Web.InitializationModule;

namespace EPi.Libraries.Favicons.Business.Initialization
{
    /// <summary>
    ///     Class Faviconinitialization.
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class FaviconInitialization : IInitializableModule
    {
        /// <summary>
        ///     The logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(FaviconInitialization));

        /// <summary>
        ///     Check if the initialization has been done.
        /// </summary>
        private static bool initialized;

        /// <summary>
        ///     Gets or sets the content events.
        /// </summary>
        /// <value>The content events.</value>
        private Injected<IContentEvents> ContentEvents { get; set; }

        /// <summary>
        ///     Gets or sets the favicon service.
        /// </summary>
        /// <value>The favicon service.</value>
        private Injected<IFaviconService> FaviconService { get; set; }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        ///     Gets called as part of the EPiServer Framework initialization sequence. Note that it will be called
        ///     only once per AppDomain, unless the method throws an exception. If an exception is thrown, the initialization
        ///     method will be called repeatedly for each request reaching the site until the method succeeds.
        /// </remarks>
        public void Initialize(InitializationEngine context)
        {
            // If there is no context, we can't do anything.
            if (context == null)
            {
                return;
            }

            // If already initialized, no need to do it again.
            if (initialized)
            {
                return;
            }

            Logger.Information("[Favicons] Initializing favicons functionality.");

            //Add initialization logic, this method is called once after CMS has been initialized
            this.ContentEvents.Service.PublishedContent += this.ServiceOnPublishedContent;

            initialized = true;

            Logger.Information("[Favicons] Favicons functionality initialized.");
        }

        /// <summary>
        ///     Resets the module into an uninitialized state.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        ///     <para>
        ///         This method is usually not called when running under a web application since the web app may be shut down very
        ///         abruptly, but your module should still implement it properly since it will make integration and unit testing
        ///         much simpler.
        ///     </para>
        ///     <para>
        ///         Any work done by
        ///         <see
        ///             cref="M:EPiServer.Framework.IInitializableModule.Initialize(EPiServer.Framework.Initialization.InitializationEngine)" />
        ///         as well as any code executing on
        ///         <see cref="E:EPiServer.Framework.Initialization.InitializationEngine.InitComplete" /> should be reversed.
        ///     </para>
        /// </remarks>
        public void Uninitialize(InitializationEngine context)
        {
            // If there is no context, we can't do anything.
            if (context == null)
            {
                return;
            }

            // If already uninitialized, no need to do it again.
            if (!initialized)
            {
                return;
            }

            Logger.Information("[Favicons] Uninitializing favicons functionality.");

            //Add uninitialization logic
            this.ContentEvents.Service.PublishedContent -= this.ServiceOnPublishedContent;

            Logger.Information("[Favicons] Favicons functionality uninitialized.");
        }

        private void ServiceOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            if (contentEventArgs == null)
            {
                return;
            }

            if (contentEventArgs.ContentLink.ID != ContentReference.StartPage.ID)
            {
                return;
            }

            ContentData contentData = contentEventArgs.Content as ContentData;

            ContentReference faviconReference =
                this.FaviconService.Service.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);

            if (ContentReference.IsNullOrEmpty(faviconReference))
            {
                this.FaviconService.Service.DeleteFavicons();
                return;
            }

            // Remove the icons. More efficient than getting them one by one and updating them.
            this.FaviconService.Service.CleanUpFavicons();

            if (!this.FaviconService.Service.CreateFavicons(faviconReference))
            {
                return;
            }

            ContentReference mobileAppIconReference =
                this.FaviconService.Service.GetPropertyValue<MobileAppIconAttribute, ContentReference>(contentData);
            this.FaviconService.Service.CreateMobileAppicons(mobileAppIconReference);
        }
    }
}
