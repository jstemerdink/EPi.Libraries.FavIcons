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
using System.Web.Mvc;

namespace EPi.Libraries.Favicons.Attributes
{
    /// <summary>
    ///     Requires that a HTTP request does not contain a trailing slash. If it does, return a 404 Not Found. This is
    ///     useful if you are dynamically generating something which acts like it's a file on the web server.
    ///     E.g. /Robots.txt/ should not have a trailing slash and should be /Robots.txt. Note, that we also don't care if
    ///     it is upper-case or lower-case in this instance.
    /// </summary>
    /// <remarks>Code based on https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate</remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class NoTrailingSlashAttribute : FilterAttribute, IAuthorizationFilter
    {
        private const char QueryCharacter = '?';

        private const char SlashCharacter = '/';

        /// <summary>
        ///     Determines whether a request contains a trailing slash and if it does, calls the
        ///     <see cref="HandleTrailingSlashRequest" /> method.
        /// </summary>
        /// <param name="filterContext">
        ///     An object that encapsulates information that is required in order to use the
        ///     <see cref="RequireHttpsAttribute" /> attribute.
        /// </param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                return;
            }

            if (filterContext.HttpContext.Request.Url == null)
            {
                return;
            }

            try
            {
                string canonicalUrl = filterContext.HttpContext.Request.Url.ToString();

                int queryIndex = canonicalUrl.IndexOf(QueryCharacter);

                if (queryIndex == -1)
                {
                    try
                    {
                        if (canonicalUrl[canonicalUrl.Length - 1] == SlashCharacter)
                        {
                            HandleTrailingSlashRequest(filterContext);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                    }
                }
                else
                {
                    try
                    {
                        if (canonicalUrl[queryIndex - 1] == SlashCharacter)
                        {
                            HandleTrailingSlashRequest(filterContext);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                    }
                }
            }
            catch (NotImplementedException)
            {
            }
        }

        /// <summary>
        ///     Handles HTTP requests that have a trailing slash but are not meant to.
        /// </summary>
        /// <param name="filterContext">
        ///     An object that encapsulates information that is required in order to use the
        ///     <see cref="RequireHttpsAttribute" /> attribute.
        /// </param>
        private static void HandleTrailingSlashRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpNotFoundResult();
        }
    }
}
