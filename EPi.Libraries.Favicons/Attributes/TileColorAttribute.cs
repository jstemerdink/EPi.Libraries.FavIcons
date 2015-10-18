using System;

namespace EPi.Libraries.Favicons.Attributes
{
    /// <summary>
    /// Class TileColorAttribute. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TileColorAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the property is used for the tile color.
        /// </summary>
        /// <value><c>true</c> if [used for the tile color]; otherwise, <c>false</c>.</value>
        public static bool TileColor
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}