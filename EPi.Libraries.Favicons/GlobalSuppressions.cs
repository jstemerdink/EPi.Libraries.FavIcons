// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Jeroen Stemerdink">
//      Copyright © 2023 Jeroen Stemerdink.
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
//
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
//
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//      SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.FaviconSettings.#ContentRepository")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Error(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetBrowserConfigXml(System.Web.Routing.RequestContext)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member", Target = "EPi.Libraries.Favicons.Business.Helpers.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member", Target = "EPi.Libraries.Favicons.Business.Helpers.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetThemeColor()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#GetTileColor()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#HasAttribute`1(System.Reflection.PropertyInfo)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.FaviconSettings.#ContentRepository")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Helpers.#HasAttribute`1(System.Reflection.PropertyInfo)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Controllers.FaviconController.#FaviconService")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Information(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#Initialize(EPiServer.Framework.Initialization.InitializationEngine)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Information(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#Uninitialize(EPiServer.Framework.Initialization.InitializationEngine)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Information(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#FaviconsUpdatedEventRaised(System.Object,EPiServer.Events.EventNotificationEventArgs)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#CreateFavicons(EPiServer.Core.ContentData)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.RaiseEvent(System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#CreateFavicons(EPiServer.Core.ContentData)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.RaiseEvent(System.String)",
        Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#CleanUpFavicons(System.String)")
]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Error(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetBrowserConfigXml(System.Web.Routing.RequestContext)"
        )]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member", Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member", Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#HasAttribute`1(System.Reflection.PropertyInfo)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetVirtualIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetIconPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetThemeColor()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetTileColor()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetFaviconSettings()")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "O", Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetPropertyValue`2(EPiServer.Core.ContentData)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetPropertyValue`2(EPiServer.Core.ContentData)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "O", Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetPropertyValue`2(EPiServer.Core.ContentReference)"
        )]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.IFaviconService.#GetPropertyValue`2(EPiServer.Core.ContentReference)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#CreateFavicons(EPiServer.Core.ContentReference)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.RaiseEvent(System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#CreateFavicons(EPiServer.Core.ContentReference)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.RaiseEvent(System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#ServiceOnPublishedContent(System.Object,EPiServer.ContentEventArgs)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#CreateFavicons(EPiServer.Core.ContentReference)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Warning(EPiServer.Logging.ILogger,System.String)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetFilePath(EPiServer.Core.ContentReference,System.String&)"
        )]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Appicons",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.IFaviconService.#CreateMobileAppicons(EPiServer.Core.ContentReference)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Error(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#GetOrCreateFolder(EPiServer.Core.ContentReference,System.String)"
        )]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#HasAttribute`1(System.Reflection.MemberInfo)"
        )]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Error(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.FaviconService.#CreateFavicon(EPiServer.Core.ContentReference,System.IO.Stream,System.String,System.Int32,System.Int32)"
        )]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.ResizeServiceBase.#GetOrCreateFaviconsFolder()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.ResizeServiceBase.#GetAssetsRootFolder()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "EPiServer.Logging.LoggerExtensions.Error(EPiServer.Logging.ILogger,System.String,System.Exception)",
        Scope = "member",
        Target =
            "EPi.Libraries.Favicons.Business.Services.ResizeServiceBase.#GetOrCreateFolder(EPiServer.Core.ContentReference,System.String)"
        )]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#ContentEvents")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#FaviconService")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#ResizeService")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#ContentRepository")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#ContentTypeRepository")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#ContentModelUsage")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#SynchronizedObjectInstanceCache")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member",
        Target = "EPi.Libraries.Favicons.Business.Services.ResizeServiceBase.#Logger")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "EPi.Libraries.Favicons.Business.Initialization.FaviconInitialization.#SynchronizedObjectInstanceCache")]
[assembly: SuppressMessage("Microsoft.Design", "CA1016:MarkAssembliesWithAssemblyVersion")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "EPi.Libraries.Favicons.Business.Services.FaviconService.#ContentCacheKeyCreator")]

