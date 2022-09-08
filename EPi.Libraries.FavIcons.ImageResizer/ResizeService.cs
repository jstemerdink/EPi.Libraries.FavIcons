// Copyright © 2022 Jeroen Stemerdink. 
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

namespace EPi.Libraries.Favicons.ImageResizer
{
    using System;
    using System.Globalization;
    using System.IO;

    using EPi.Libraries.Favicons.Business.Services;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.DataAccess;
    using EPiServer.Framework.Blobs;
    using EPiServer.Logging;
    using EPiServer.ServiceLocation;

    using Imageflow.Bindings;
    using Imageflow.Fluent;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    ///     Class ResizeService.
    /// </summary>
    /// <seealso cref="IResizeService" />
    /// <seealso cref="ResizeServiceBase" />
    [ServiceConfiguration(typeof(IResizeService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ResizeService : ResizeServiceBase
    {
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
            Type mediaType = this.ContentMediaResolver.Service.GetFirstMatching(".png");

            ContentType contentType = this.ContentTypeRepository.Service.Load(mediaType);

            try
            {
                if (imageBytes.Length == 0)
                {
                    Logger.Debug("[Favicons] Error creating icon. Original file is empty.");
                    return;
                }

                ArraySegment<byte>? processedImageData = this.ProcessImage(imageBytes, width, height);

                if (!processedImageData.HasValue)
                {
                    Logger.Debug("[Favicons] Error creating icon. Processed file is empty.");
                    return;
                }

                byte[] processedImageBytes = processedImageData.Value.Array;

                if (processedImageBytes?.Length == 0)
                {
                    Logger.Debug("[Favicons] Error creating icon. Processed file is empty.");
                    return;
                }

                // Get a new empty file data
                ImageData media = this.ContentRepository.Service.GetDefault<ImageData>(rootFolder, contentType.ID);

                media.Name = string.Format(CultureInfo.InvariantCulture, "{0}-{1}x{2}.png", filePrefix, width, height);

                // Create a blob in the binary container
                Blob blob = this.BlobFactory.Service.CreateBlob(media.BinaryDataContainer, ".png");
                
                blob.WriteAllBytes(processedImageBytes);

                // Assign to file and publish changes
                media.BinaryData = blob;
                this.ContentRepository.Service.Save(media, SaveAction.Publish);
            }
            catch (AccessDeniedException accessDeniedException)
            {
                Logger.Error("[Favicons] Error creating icon.", accessDeniedException);
            }
            catch (ArgumentNullException argumentNullException)
            {
                Logger.Error("[Favicons] Error creating icon.", argumentNullException);
            }
            catch (FormatException formatException)
            {
                Logger.Error("[Favicons] Error creating icon.", formatException);
            }
            catch (Exception exception)
            {
                Logger.Error("[Favicons] Error creating icon.", exception);
            }
        }

        private ArraySegment<byte>? ProcessImage(byte[] imageBytes, int width, int height)
        {
            using (ImageJob b = new())
            {
                BuildNode buildNode = b.Decode(imageBytes);

                BuildJobResult r = buildNode
                    .ResizerCommands($"width={width}&height={height}&crop=auto&format=png")
                    .EncodeToBytes(new PngQuantEncoder(100, 80))
                    .Finish()
                    .InProcessAsync()
                    .Result;

                return r.First.TryGetBytes();
            }
        }
    }
}
