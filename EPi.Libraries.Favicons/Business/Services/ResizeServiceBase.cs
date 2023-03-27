// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResizeServiceBase.cs" company="Jeroen Stemerdink">
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

namespace EPi.Libraries.Favicons.Business.Services
{
    using System;
    using System.IO;
    using System.Linq;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.DataAccess;
    using EPiServer.Framework.Blobs;
    using EPiServer.Security;
    using EPiServer.Web;

    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Abstract Class ResizeService.
    /// </summary>
    /// <seealso cref="EPi.Libraries.Favicons.Business.Services.IResizeService" />
    public abstract class ResizeServiceBase : IResizeService
    {
        private readonly ILogger<ResizeServiceBase> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeServiceBase" /> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="contentMediaResolver">The content media resolver.</param>
        /// <param name="blobFactory">The BLOB factory.</param>
        /// <param name="logger">The logger.</param>
        protected ResizeServiceBase(
            IContentRepository contentRepository,
            IContentTypeRepository contentTypeRepository,
            ContentMediaResolver contentMediaResolver,
            IBlobFactory blobFactory,
            ILogger<ResizeServiceBase> logger)
        {
            this.ContentRepository = contentRepository;
            this.ContentTypeRepository = contentTypeRepository;
            this.ContentMediaResolver = contentMediaResolver;
            this.BlobFactory = blobFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the BLOB factory.
        /// </summary>
        /// <value>The BLOB factory.</value>
        public IBlobFactory BlobFactory { get; }

        /// <summary>
        /// Gets the content media resolver.
        /// </summary>
        /// <value>The content media resolver.</value>
        public ContentMediaResolver ContentMediaResolver { get; }

        /// <summary>
        /// Gets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        public IContentRepository ContentRepository { get; }

        /// <summary>
        /// Gets the content type repository.
        /// </summary>
        /// <value>The content type repository.</value>
        public IContentTypeRepository ContentTypeRepository { get; }

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        public virtual void CleanUpFavicons()
        {
            ContentReference faviconsFolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(contentLink: faviconsFolder))
            {
                return;
            }

            this.ContentRepository.DeleteChildren(contentLink: faviconsFolder, true, access: AccessLevel.NoAccess);
        }

        /// <summary>
        ///     Creates the favicon.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="originalFile">The original file.</param>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public abstract void CreateFavicon(
            ContentReference rootFolder,
            byte[] originalFile,
            string filePrefix,
            int width,
            int height);

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if creating the favicons succeeded, <c>false</c> otherwise.</returns>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Either the input stream or the destination stream was closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
        /// <exception cref="NotSupportedException">The current stream does not support reading does not support writing.</exception>
        /// <exception cref="ArgumentNullException">Either the input stream or the destination stream is <see langword="null" />.</exception>
        public virtual bool CreateFavicons(ContentReference iconReference)
        {
            if (ContentReference.IsNullOrEmpty(contentLink: iconReference))
            {
                return false;
            }

            ContentReference rootfolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(contentLink: rootfolder))
            {
                return false;
            }

            ImageData faviconImageData;

            if (!this.ContentRepository.TryGet(contentLink: iconReference, content: out faviconImageData))
            {
                return false;
            }

            byte[] s = ReadStreamFully(faviconImageData.BinaryData.OpenRead());

            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 57, 57);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 60, 60);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 72, 72);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 76, 76);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 114, 114);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 120, 120);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 144, 144);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 152, 152);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-icon", 180, 180);

            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "mstile", 70, 70);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "mstile", 150, 150);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "mstile", 310, 310);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "mstile", 310, 150);

            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 36, 36);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 48, 48);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 72, 72);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 96, 96);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 144, 144);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "android-chrome", 192, 192);

            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "favicon", 16, 16);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "favicon", 32, 32);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "favicon", 96, 96);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "favicon", 192, 192);

            return true;
        }

        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Either the input stream or the destination stream was closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
        /// <exception cref="NotSupportedException">The current stream does not support reading does not support writing.</exception>
        /// <exception cref="ArgumentNullException">Either the input stream or the destination stream is <see langword="null" />.</exception>
        public virtual void CreateMobileAppIcons(ContentReference iconReference)
        {
            if (ContentReference.IsNullOrEmpty(contentLink: iconReference))
            {
                return;
            }

            ContentReference rootfolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(contentLink: rootfolder))
            {
                return;
            }

            ImageData faviconImageData;

            if (!this.ContentRepository.TryGet(contentLink: iconReference, content: out faviconImageData))
            {
                return;
            }

            byte[] s = ReadStreamFully(faviconImageData.BinaryData.OpenRead());

            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 1536, 2008);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 1496, 2048);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 768, 1004);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 748, 1024);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 640, 1096);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 640, 1096);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 640, 920);
            this.CreateFavicon(rootFolder: rootfolder, originalFile: s, "apple-touch-startup-image", 320, 460);
        }

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        public virtual void DeleteFavicons()
        {
            ContentReference faviconsFolder = this.GetOrCreateFaviconsFolder();

            if (ContentReference.IsNullOrEmpty(contentLink: faviconsFolder))
            {
                return;
            }

            this.ContentRepository.Delete(contentLink: faviconsFolder, true);
        }

        /// <summary>
        ///     Gets the assets root folder.
        /// </summary>
        /// <returns>The <see cref="ContentReference"/> for the root folder of the assets.</returns>
        protected static ContentReference GetAssetsRootFolder()
        {
            ContentReference rootFolder = SiteDefinition.Current.SiteAssetsRoot;

            if (ContentReference.IsNullOrEmpty(contentLink: rootFolder))
            {
                rootFolder = SiteDefinition.Current.GlobalAssetsRoot;
            }

            return rootFolder;
        }

        /// <summary>
        /// Reads the stream fully.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A byte array.</returns>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="NotSupportedException">The current stream does not support reading does not support writing.</exception>
        /// <exception cref="ArgumentNullException">Either the input stream or the destination stream is <see langword="null" />.</exception>
        /// <exception cref="ObjectDisposedException">Either the input stream or the destination stream was closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
        protected static byte[] ReadStreamFully(Stream input)
        {
            if (input == null)
            {
                return Array.Empty<byte>();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(destination: ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Gets the or create favicons folder.
        /// </summary>
        /// <returns>The <see cref="ContentReference"/> for the folder of the favicons.</returns>
        protected ContentReference GetOrCreateFaviconsFolder()
        {
            ContentReference rootFolder = GetAssetsRootFolder();

            ContentFolder faviconsFolder = this.GetOrCreateFolder(parentFolder: rootFolder, "Favicons");

            return faviconsFolder == null ? ContentReference.EmptyReference : faviconsFolder.ContentLink;
        }

        /// <summary>
        ///     Returns a <c>ContentFolder</c> folder
        /// </summary>
        /// <param name="parentFolder">The folder container.</param>
        /// <param name="folderName">Identifier for folder.</param>
        /// <returns>Stored <c>ContentFolder</c> folder; otherwise created folder.</returns>
        protected ContentFolder GetOrCreateFolder(ContentReference parentFolder, string folderName)
        {
            ContentFolder storedFolder = this.ContentRepository.GetChildren<ContentFolder>(contentLink: parentFolder)
                .FirstOrDefault(
                    f => string.Compare(
                             strA: f.Name,
                             strB: folderName,
                             comparisonType: StringComparison.OrdinalIgnoreCase) == 0);

            if (storedFolder != null)
            {
                return storedFolder;
            }

            ContentFolder parent;

            if (!this.ContentRepository.TryGet(contentLink: parentFolder, content: out parent))
            {
                return null;
            }

            try
            {
                ContentFolder folder = this.ContentRepository.GetDefault<ContentFolder>(parentLink: parent.ContentLink);
                folder.Name = folderName;

                ContentReference folderReference = this.ContentRepository.Save(
                    content: folder,
                    action: SaveAction.Publish,
                    access: AccessLevel.NoAccess);

                ContentFolder newFolder;

                return !this.ContentRepository.TryGet(contentLink: folderReference, content: out newFolder)
                           ? null
                           : newFolder;
            }
            catch (AccessDeniedException accessDeniedException)
            {
                this.logger.Log(
                    logLevel: LogLevel.Error,
                    exception: accessDeniedException,
                    "[Favicons] Error creating content folder.");
                return null;
            }
        }
    }
}