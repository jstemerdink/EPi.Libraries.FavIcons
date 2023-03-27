// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResizeService.cs" company="Jeroen Stemerdink">
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

namespace EPi.Libraries.Favicons.ImageProcessor
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;

    using EPi.Libraries.Favicons.Business.Services;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.DataAccess;
    using EPiServer.Framework.Blobs;
    using EPiServer.ServiceLocation;

    using global::ImageProcessor;
    using global::ImageProcessor.Imaging.Formats;

    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class ResizeService.
    /// </summary>
    /// <seealso cref="IResizeService" />
    /// <seealso cref="ResizeServiceBase" />
    [ServiceConfiguration(typeof(IResizeService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ResizeService : ResizeServiceBase
    {
        private readonly ILogger<ResizeService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeService" /> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="contentMediaResolver">The content media resolver.</param>
        /// <param name="blobFactory">The BLOB factory.</param>
        /// <param name="logger">The logger.</param>
        public ResizeService(
            IContentRepository contentRepository,
            IContentTypeRepository contentTypeRepository,
            ContentMediaResolver contentMediaResolver,
            IBlobFactory blobFactory,
            ILogger<ResizeService> logger)
            : base(
                contentRepository: contentRepository,
                contentTypeRepository: contentTypeRepository,
                contentMediaResolver: contentMediaResolver,
                blobFactory: blobFactory,
                logger: logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///     Creates the favicon.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="imageBytes">The original file.</param>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public override void CreateFavicon(
            ContentReference rootFolder,
            byte[] imageBytes,
            string filePrefix,
            int width,
            int height)
        {
            // Get a suitable MediaData type from extension
            Type mediaType = this.ContentMediaResolver.GetFirstMatching(".png");

            ContentType contentType = this.ContentTypeRepository.Load(modelType: mediaType);

            try
            {
                // Get a new empty file data
                ImageData media = this.ContentRepository.GetDefault<ImageData>(
                    parentLink: rootFolder,
                    contentTypeID: contentType.ID);

                media.Name = string.Format(
                    provider: CultureInfo.InvariantCulture,
                    "{0}-{1}x{2}.png",
                    arg0: filePrefix,
                    arg1: width,
                    arg2: height);

                // Create a blob in the binary container
                Blob blob = this.BlobFactory.CreateBlob(id: media.BinaryDataContainer, ".png");

                ISupportedImageFormat format = new PngFormat();
                Size size = new Size(width: width, height: height);

                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(bytes: imageBytes).Resize(size: size).Format(format: format)
                            .Save(stream: outStream);
                    }

                    // Assign to file and publish changes
                    media.BinaryData = blob;
                    this.ContentRepository.Save(content: media, action: SaveAction.Publish);
                }
            }
            catch (AccessDeniedException accessDeniedException)
            {
                this.logger.Log(
                    logLevel: LogLevel.Error,
                    exception: accessDeniedException,
                    "[Favicons] Error creating icon.");
            }
            catch (ArgumentNullException argumentNullException)
            {
                this.logger.Log(
                    logLevel: LogLevel.Error,
                    exception: argumentNullException,
                    "[Favicons] Error creating icon.");
            }
            catch (FormatException formatException)
            {
                this.logger.Log(
                    logLevel: LogLevel.Error,
                    exception: formatException,
                    "[Favicons] Error creating icon.");
            }
        }
    }
}