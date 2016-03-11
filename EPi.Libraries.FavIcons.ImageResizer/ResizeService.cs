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

using ImageResizer;

namespace EPi.Libraries.FavIcons.ImageResizer
{
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
        /// <param name="originalFile">The original file.</param>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public override void CreateFavicon(
            ContentReference rootFolder,
            Stream originalFile,
            string filePrefix,
            int width,
            int height)
        {
            //Get a suitable MediaData type from extension
            Type mediaType = this.ContentMediaResolver.Service.GetFirstMatching(".png");

            ContentType contentType = this.ContentTypeRepository.Service.Load(mediaType);

            try
            {
                //Get a new empty file data
                ImageData media = this.ContentRepository.Service.GetDefault<ImageData>(rootFolder, contentType.ID);

                media.Name = string.Format(CultureInfo.InvariantCulture, "{0}-{1}x{2}.png", filePrefix, width, height);

                //Create a blob in the binary container
                Blob blob = this.BlobFactory.Service.CreateBlob(media.BinaryDataContainer, ".png");

                ImageJob imageJob = new ImageJob(
                    originalFile,
                    blob.OpenWrite(),
                    new Instructions(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "width={0}&height={1}&crop=auto&format=png",
                            width,
                            height)))
                                        {
                                            ResetSourceStream = true,
                                            DisposeDestinationStream = true,
                                            DisposeSourceObject = false
                                        };

                imageJob.Build();

                //Assign to file and publish changes
                media.BinaryData = blob;
                this.ContentRepository.Service.Save(media, SaveAction.Publish);
            }
            catch (AccessDeniedException accessDeniedException)
            {
                Logger.Error("[Favicons] Error creating icon.", accessDeniedException);
            }
        }
    }
}