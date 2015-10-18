using System;

namespace EPi.Libraries.Favicons.Attributes
{
    /// <summary>
    /// Class WebsiteIconAttribute. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class WebsiteIconAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the property is used for the website icons.
        /// </summary>
        /// <value><c>true</c> if [used for the website icons]; otherwise, <c>false</c>.</value>
        public static bool WebsiteIcon
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}