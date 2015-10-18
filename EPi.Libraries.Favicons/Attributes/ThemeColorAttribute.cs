using System;

namespace EPi.Libraries.Favicons.Attributes
{
    /// <summary>
    /// Class ThemeColorAttribute. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ThemeColorAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the property is used for the theme color.
        /// </summary>
        /// <value><c>true</c> if [used for the theme color]; otherwise, <c>false</c>.</value>
        public static bool ThemeColor
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}