#Region "Microsoft.VisualBasic::c312510d7c70f3800cf5423079d9e90e, WebCloud\SMRUCC.HTTPInternal\AppEngine\CookieParser.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Module CookieParser
' 
'         Function: FindIndex, isPathDomainOrDate, ParseOneNameAndValue, ParseSetCookie
' 
' 
' /********************************************************************************/

#End Region

'
' * Solution for CookieContainer bug in .NET 3.5
' *
' * Usage:
' * httpWebRequest.Headers[HttpRequestHeader.Cookie] = CookieParser.ParseSetCookie(httpWebResponse.Headers[HttpResponseHeader.SetCookie]);
' *
' * Created by LYF610400210
'

Imports System.Collections.Specialized

Namespace Core.Message

    Public Class Cookies

        Public Shared Function GetCookies(cookies As String) As NameValueCollection

        End Function

    End Class
End Namespace
