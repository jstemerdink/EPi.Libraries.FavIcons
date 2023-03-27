﻿// --------------------------------------------------------------------------------------------------------------------
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

namespace EPi.Libraries.Favicons.ImageResizer
{
    using System;
    using System.Globalization;

    using EPi.Libraries.Favicons.Business.Services;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.DataAccess;
    using EPiServer.Framework.Blobs;
    using EPiServer.ServiceLocation;

    using Imageflow.Fluent;

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
                if (imageBytes.Length == 0)
                {
                    this.logger.Log(
                        logLevel: LogLevel.Debug,
                        "[Favicons] Error creating icon. Original file is empty.");
                    return;
                }

                ArraySegment<byte>? processedImageData = this.ProcessImage(
                    imageBytes: imageBytes,
                    width: width,
                    height: height);

                if (!processedImageData.HasValue)
                {
                    this.logger.Log(
                        logLevel: LogLevel.Debug,
                        "[Favicons] Error creating icon. Processed file is empty.");
                    return;
                }

                byte[] processedImageBytes = processedImageData.Value.Array;

                if (processedImageBytes?.Length == 0)
                {
                    this.logger.Log(
                        logLevel: LogLevel.Debug,
                        "[Favicons] Error creating icon. Processed file is empty.");
                    return;
                }

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

                blob.WriteAllBytes(data: processedImageBytes);

                // Assign to file and publish changes
                media.BinaryData = blob;
                this.ContentRepository.Save(content: media, action: SaveAction.Publish);
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
            catch (Exception exception)
            {
                this.logger.Log(logLevel: LogLevel.Error, exception: exception, "[Favicons] Error creating icon.");
            }
        }

        private ArraySegment<byte>? ProcessImage(byte[] imageBytes, int width, int height)
        {
            using (ImageJob b = new())
            {
                BuildNode buildNode = b.Decode(source: imageBytes);

                BuildJobResult r = buildNode.ResizerCommands($"width={width}&height={height}&crop=auto&format=png")
                    .EncodeToBytes(new PngQuantEncoder(100, 80)).Finish().InProcessAsync().Result;

                return r.First.TryGetBytes();
            }
        }
    }
}