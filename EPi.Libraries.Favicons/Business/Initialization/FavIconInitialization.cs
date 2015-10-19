// Copyright© 2015 Jeroen Stemerdink. 
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
using System.IO;

using EPi.Libraries.Favicons.Attributes;
using EPi.Libraries.Favicons.Business.Services;
using EPi.Libraries.Favicons.Models;

using EPiServer;
using EPiServer.Core;
using EPiServer.Events;
using EPiServer.Events.Clients;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;

using ImageResizer;

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
        ///     FaviconsCreated
        /// </summary>
        private const string FaviconsCreated = "FaviconsCreated";

        /// <summary>
        ///     FaviconsDeleted
        /// </summary>
        private const string FaviconsDeleted = "FaviconsDeleted";

        // Generate unique id for the raiser.
        private static readonly Guid FaviconRaiserId = new Guid("8bc36f59-3167-4859-aa5d-61ef76a999de");

        // Generate unique id for the reload event.
        private static readonly Guid FaviconUpdatedEventId = new Guid("ad76bc78-0d3b-4049-a8da-a90a0d035e26");

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
        ///     Gets or sets the event service.
        /// </summary>
        /// <value>The event service.</value>
        private Injected<IEventRegistry> EventService { get; set; }

        /// <summary>
        ///     Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        private Injected<IContentRepository> ContentRepository { get; set; }

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
        ///     method will be called repeadetly for each request reaching the site until the method succeeds.
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

            // Make sure the RemoteCacheSynchronization event is registered before the custome event.
            this.EventService.Service.Get(RemoteCacheSynchronization.RemoveFromCacheEventId);

            // Attach a custom event to create the icons on another server, eg. in LoadBalanced environments.
            Event faviconssUpdated = this.EventService.Service.Get(FaviconUpdatedEventId);
            faviconssUpdated.Raised += this.FaviconsUpdatedEventRaised;

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
        }

        private void FaviconsUpdatedEventRaised(object sender, EventNotificationEventArgs e)
        {
            // We don't want to process events raised on this machine so we will check the raiser id.
            if (e.RaiserId == FaviconRaiserId)
            {
                return;
            }

            string eventMessage = e.Param as string;

            if (string.IsNullOrWhiteSpace(eventMessage))
            {
                return;
            }

            if (eventMessage.Equals(FaviconsCreated))
            {
                ContentData contentData;
                this.ContentRepository.Service.TryGet(SiteDefinition.Current.StartPage, out contentData);
                this.CreateFavicons(contentData);
            }

            if (eventMessage.Equals(FaviconsDeleted))
            {
                FaviconSettings faviconSettings = this.FaviconService.Service.GetFaviconSettings();
                this.CleanUpFavicons(faviconSettings.FaviconsPath);
            }

            Logger.Information("[Favicons] Favicons created or deleted on other machine.");
        }

        private void RaiseEvent(string message)
        {
            // Raise the FaviconsUpdated event.
            this.EventService.Service.Get(FaviconUpdatedEventId).Raise(FaviconRaiserId, message);
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

            this.CreateFavicons(contentData);

            FaviconSettings faviconSettings = this.FaviconService.Service.GetFaviconSettings();

            if (!faviconSettings.FaviconsExist)
            {
                this.CleanUpFavicons(faviconSettings.FaviconsPath);
            }
        }

        private void CreateFavicons(ContentData contentData)
        {
            ContentReference iconReference =
                this.FaviconService.Service.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);

            string iconsPath = this.FaviconService.Service.GetIconPath();

            if (ContentReference.IsNullOrEmpty(iconReference))
            {
                return;
            }

            ImageData iconFile;

            this.ContentRepository.Service.TryGet(iconReference, out iconFile);

            if (iconFile == null)
            {
                Logger.Warning("[Favicons] Icon file not found.");
                return;
            }

            FileBlob binaryData = iconFile.BinaryData as FileBlob;

            if (binaryData == null)
            {
                Logger.Warning("[Favicons] Icon is not a file.");
                return;
            }

            string filePath = binaryData.FilePath;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Warning("[Favicons] Icon file has no path.");
                return;
            }

            if (string.IsNullOrWhiteSpace(iconsPath))
            {
                return;
            }

            if (!Directory.Exists(iconsPath))
            {
                Directory.CreateDirectory(iconsPath);
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

            this.RaiseEvent(FaviconsCreated);
        }

        private void CleanUpFavicons(string iconsPath)
        {
            if (!this.FaviconService.Service.IconPathExists(iconsPath))
            {
                return;
            }

            Directory.Delete(iconsPath, true);

            this.RaiseEvent(FaviconsDeleted);
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
