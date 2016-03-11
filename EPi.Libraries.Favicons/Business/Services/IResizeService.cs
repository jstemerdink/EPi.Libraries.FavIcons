using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPiServer.Core;

namespace EPi.Libraries.Favicons.Business.Services
{
    /// <summary>
    /// Interface IResizeService
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
