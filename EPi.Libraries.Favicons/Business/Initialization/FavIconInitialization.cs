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

using System.Globalization;
using System.IO;

using EPi.Libraries.Favicons.Attributes;

using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

using ImageResizer;

using InitializationModule = EPiServer.Web.InitializationModule;

namespace EPi.Libraries.Favicons.Business.Initialization
{
    /// <summary>
    /// Class FaviconInitialization.
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class FaviconInitialization : IInitializableModule
    {
        /// <summary>
        /// Gets or sets the content events.
        /// </summary>
        /// <value>The content events.</value>
        protected Injected<IContentEvents> ContentEvents { get; set; }

        /// <summary>
        /// Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        protected Injected<IContentRepository> ContentRepository { get; set; }

        /// <summary>
        /// Gets or sets the BLOB factory.
        /// </summary>
        /// <value>The BLOB factory.</value>
        protected Injected<BlobFactory> BlobFactory { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>Gets called as part of the EPiServer Framework initialization sequence. Note that it will be called
        /// only once per AppDomain, unless the method throws an exception. If an exception is thrown, the initialization
        /// method will be called repeadetly for each request reaching the site until the method succeeds.</remarks>
        public void Initialize(InitializationEngine context)
        {
            //Add initialization logic, this method is called once after CMS has been initialized
            this.ContentEvents.Service.PublishedContent += this.ServiceOnPublishedContent;
        }

        /// <summary>
        /// Resets the module into an uninitialized state.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks><para>
        /// This method is usually not called when running under a web application since the web app may be shut down very
        /// abruptly, but your module should still implement it properly since it will make integration and unit testing
        /// much simpler.
        /// </para>
        /// <para>
        /// Any work done by <see cref="M:EPiServer.Framework.IInitializableModule.Initialize(EPiServer.Framework.Initialization.InitializationEngine)" /> as well as any code executing on <see cref="E:EPiServer.Framework.Initialization.InitializationEngine.InitComplete" /> should be reversed.
        /// </para></remarks>
        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
            this.ContentEvents.Service.PublishedContent -= this.ServiceOnPublishedContent;
        }

        /// <summary>
        /// Services the content of the on published.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="contentEventArgs">The <see cref="ContentEventArgs"/> instance containing the event data.</param>
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

            this.CreateFavIcons(contentData);

            SetThemeColor(contentData);
            SetTileColor(contentData);
        }

        /// <summary>
        /// Sets the color of the theme.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        private static void SetThemeColor(ContentData contentData)
        {
            string themeColor = Helpers.GetPropertyValue<ThemeColorAttribute, string>(contentData);
            
            if (!string.IsNullOrWhiteSpace(themeColor))
            {
                FaviconSettings.Instance.ThemeColor = themeColor;
            }
        }

        /// <summary>
        /// Sets the color of the tile.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        private static void SetTileColor(ContentData contentData)
        {
            string tileColor = Helpers.GetPropertyValue<TileColorAttribute, string>(contentData);

            if (!string.IsNullOrWhiteSpace(tileColor))
            {
                FaviconSettings.Instance.TileColor = tileColor;
            }
        }

        /// <summary>
        /// Creates the fav icons.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        private void CreateFavIcons(ContentData contentData)
        {
            ContentReference iconReference =
                Helpers.GetPropertyValue<WebsiteIconAttribute, ContentReference>(contentData);

            string iconsPath = Helpers.GetIconPath();

            if (ContentReference.IsNullOrEmpty(iconReference))
            {
                FaviconSettings.Instance.FaviconsExist = false;
                CleanUpFavIcons(iconsPath);
                return;
            }

            ImageData iconFile;

            this.ContentRepository.Service.TryGet(iconReference, out iconFile);

            if (iconFile == null)
            {
                FaviconSettings.Instance.FaviconsExist = false;
                CleanUpFavIcons(iconsPath);
                return;
            }

            FileBlob binaryData = iconFile.BinaryData as FileBlob;

            if (binaryData == null)
            {
                FaviconSettings.Instance.FaviconsExist = false;
                CleanUpFavIcons(iconsPath);
                return;
            }

            string filePath = binaryData.FilePath;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                FaviconSettings.Instance.FaviconsExist = false;
                CleanUpFavIcons(iconsPath);
                return;
            }

            if (string.IsNullOrWhiteSpace(iconsPath))
            {
                FaviconSettings.Instance.FaviconsExist = false;
                CleanUpFavIcons(iconsPath);
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

            CreateFavIcon(filePath, imageBuilder, iconsPath, 16, 16);
            CreateFavIcon(filePath, imageBuilder, iconsPath, 32, 32);
            CreateFavIcon(filePath, imageBuilder, iconsPath, 96, 96);
            CreateFavIcon(filePath, imageBuilder, iconsPath, 192, 192);

            FaviconSettings.Instance.FaviconsExist = true;
        }

        /// <summary>
        /// Cleans up fav icons.
        /// </summary>
        /// <param name="iconsPath">The icons path.</param>
        private static void CleanUpFavIcons(string iconsPath)
        {
            if (!Helpers.IconPathExists(iconsPath))
            {
                return;
            }

            Directory.Delete(iconsPath, true);
        }

        /// <summary>
        /// Creates the apple icon.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="imageBuilder">The image builder.</param>
        /// <param name="iconsPath">The icons path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
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

        /// <summary>
        /// Creates the ms tile icon.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="imageBuilder">The image builder.</param>
        /// <param name="iconsPath">The icons path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
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

        /// <summary>
        /// Creates the android icon.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="imageBuilder">The image builder.</param>
        /// <param name="iconsPath">The icons path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
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

        /// <summary>
        /// Creates the fav icon.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="imageBuilder">The image builder.</param>
        /// <param name="iconsPath">The icons path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private static void CreateFavIcon(
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
