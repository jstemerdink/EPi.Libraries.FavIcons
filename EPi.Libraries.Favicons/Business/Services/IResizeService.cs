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

using EPiServer.Core;

namespace EPi.Libraries.Favicons.Business.Services
{
    /// <summary>
    ///     Interface IResizeService
    /// </summary>
    public interface IResizeService
    {
        /// <summary>
        ///     Creates the favicons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CreateFavicons(ContentReference iconReference);

        /// <summary>
        ///     Creates the mobile app icons.
        /// </summary>
        /// <param name="iconReference">The icon reference.</param>
        void CreateMobileAppIcons(ContentReference iconReference);

        /// <summary>
        ///     Cleans up favicons.
        /// </summary>
        void CleanUpFavicons();

        /// <summary>
        ///     Deletes the favicons.
        /// </summary>
        void DeleteFavicons();
    }
}
