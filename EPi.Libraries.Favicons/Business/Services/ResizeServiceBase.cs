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
using System.IO;
using System.Linq;

using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPi.Libraries.Favicons.Business.Services
{
    /// <summary>
    /// Abstract Class ResizeService.
    /// </summary>
    /// <seealso cref="EPi.Libraries.Favicons.Business.Services.IResizeService" />
    public abstract class ResizeServiceBase : IResizeService
    {
        /// <summary>
        ///     The logger
        /// </summary>
        protected static readonly ILogger Logger = LogManager.GetLogger();

        /// <summary>
        ///     Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        protected Injected<IContentRepository> ContentRepository { get; set; }

        /// <summary>
        ///     Gets or sets the content type repository.
        /// </summary>
        /// <value>The content type repository.</value>
        protected Injected<IContentTypeRepository> ContentTypeRepository { get; set; }

        /// <summary>
        ///     Gets or sets the content media resolver.
        /// </summary>
        /// <value>The content media resolver.</value>
        protected Injected<ContentMediaResolver> ContentMediaResolver { get; set; }

        /// <summary>
        ///     Gets or sets the BLOB factory.
        /// </summary>
        /// <value>The BLOB factory.</value>
        protected Injected<BlobFactory> BlobFactory { get; set; }

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool CreateFavicons(ContentReference iconReference)
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
        public virtual void CreateMobileAppicons(ContentReference iconReference)
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
        public virtual void CleanUpFavicons()
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
        public virtual void DeleteFavicons()
        {
            ContentReference faviconsFolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(faviconsFolder))
            {
                return;
            }

            this.ContentRepository.Service.Delete(faviconsFolder, true);
        }

        /// <summary>
        /// Creates the favicon.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="originalFile">The original file.</param>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public abstract void CreateFavicon(ContentReference rootFolder, Stream originalFile, string filePrefix, int width, int height);
        

        /// <summary>
        ///     Gets the or create favicons folder.
        /// </summary>
        /// <returns>ContentReference.</returns>
        protected ContentReference GetOrCreateFaviconsFolder()
        {
            ContentReference rootFolder = GetAssetsRootFolder();

            ContentFolder faviconsFolder = this.GetOrCreateFolder(rootFolder, "Favicons");

            return faviconsFolder == null ? ContentReference.EmptyReference : faviconsFolder.ContentLink;
        }

        /// <summary>
        /// Gets the assets root folder.
        /// </summary>
        /// <returns>ContentReference.</returns>
        protected static ContentReference GetAssetsRootFolder()
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
        protected ContentFolder GetOrCreateFolder(ContentReference parentFolder, string folderName)
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
    }
}