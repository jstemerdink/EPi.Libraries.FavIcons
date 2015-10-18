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

using EPi.Libraries.Favicons.Attributes;

using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPi.Libraries.Favicons.Business
{
    /// <summary>
    /// The FavIconSettings class
    /// </summary>
    /// <examples>
    /// FavIconSettings singleton = FavIconSettings.Instance;
    /// </examples>
    public sealed class FaviconSettings
    {
        /// <summary>
        /// The synclock object.
        /// </summary>
        private static readonly object SyncLock = new object();

        /// <summary>
        /// Gets or sets the content repository.
        /// </summary>
        /// <value>The content repository.</value>
        private Injected<IContentRepository> ContentRepository { get; set; }

        /// <summary>
        /// The one and only FavIconSettings instance.
        /// </summary>
        private static volatile FaviconSettings instance;

        /// <summary>
        /// Gets or sets the color of the theme.
        /// </summary>
        /// <value>The color of the theme.</value>
        public string ThemeColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the tile.
        /// </summary>
        /// <value>The color of the tile.</value>
        public string TileColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [fav icons exist].
        /// </summary>
        /// <value><c>true</c> if [fav icons exist]; otherwise, <c>false</c>.</value>
        public bool FaviconsExist { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="FaviconSettings" /> class from being created.
        /// </summary>
        private FaviconSettings()
        {
            ContentData contentData;
            this.ContentRepository.Service.TryGet(SiteDefinition.Current.StartPage, out contentData);

            this.ThemeColor = Helpers.GetPropertyValue<ThemeColorAttribute, string>(contentData);
            this.TileColor = Helpers.GetPropertyValue<TileColorAttribute, string>(contentData);
            this.FaviconsExist = Helpers.IconPathExists();
        }

        /// <summary>
        /// Gets the instance of the FavIconSettings object.
        /// </summary>
        /// <value>The instance.</value>
        public static FaviconSettings Instance
        {
            get
            {
                // Double checked locking
                if (instance != null)
                {
                    return instance;
                }

                lock (SyncLock)
                {
                    if (instance == null)
                    {
                        instance = new FaviconSettings();
                    }
                }

                return instance;
            }
        }
    }
}