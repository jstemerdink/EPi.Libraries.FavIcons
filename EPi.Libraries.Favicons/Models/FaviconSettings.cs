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

namespace EPi.Libraries.Favicons.Models
{
    /// <summary>
    ///     Class FaviconSettings.
    /// </summary>
    public class FaviconSettings
    {
        /// <summary>
        ///     Gets or sets the color of the theme.
        /// </summary>
        /// <value>The color of the theme.</value>
        public string ThemeColor { get; set; }

        /// <summary>
        ///     Gets or sets the color of the tile.
        /// </summary>
        /// <value>The color of the tile.</value>
        public string TileColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [to display the favicons].
        /// </summary>
        /// <value><c>true</c> if [to display the favicons]; otherwise, <c>false</c>.</value>
        public bool DisplayFavicons { get; set; }

        /// <summary>
        ///     Gets or sets the favicons path.
        /// </summary>
        /// <value>The favicons path.</value>
        public string FaviconsPath { get; set; }

        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the short name of the application.
        /// </summary>
        /// <value>The short name of the application.</value>
        public string ApplicationShortName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [mobile web application capable].
        /// </summary>
        /// <value><c>true</c> if [mobile web application capable]; otherwise, <c>false</c>.</value>
        public bool MobileWebAppCapable { get; set; }
    }
}
