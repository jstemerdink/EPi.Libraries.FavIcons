using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPi.Libraries.Favicons.Attributes
{
    /// <summary>
    /// Class ContainsSettingsAttribute. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContainsSettingsAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether [contains settings].
        /// </summary>
        /// <value><c>true</c> if [contains settings]; otherwise, <c>false</c>.</value>
        public static bool ContainsSettings
        {
            get
            {
                return true;
            }
        }
    }
}