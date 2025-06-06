﻿#Region "Microsoft.VisualBasic::1a41f37aaf81e4da1bc9e3cf0c905419, G:/GCModeller/src/runtime/httpd/src/Flute//HttpMessage/HttpRequest.vb"

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


    ' Code Statistics:

    '   Total Lines: 107
    '    Code Lines: 62
    ' Comment Lines: 31
    '   Blank Lines: 14
    '     File Size: 3.69 KB


    '     Class HttpRequest
    ' 
    '         Properties: HttpHeaders, HTTPMethod, HttpRequest, IsWWWRoot, Remote
    '                     URL, version
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetBoolean, GetCookies, HasValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Flute.Http.Core.Message.HttpHeader
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Core.Message

    ''' <summary>
    ''' Data of the http request
    ''' </summary>
    Public Class HttpRequest

        ''' <summary>
        ''' GET/POST/PUT/DELETE....
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' http方法名是大写的
        ''' </remarks>
        Public ReadOnly Property HTTPMethod As String
        Public ReadOnly Property URL As URL
        ''' <summary>
        ''' <see cref="HttpProcessor.http_protocol_versionstring"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property version As String
        Public ReadOnly Property HttpHeaders As Dictionary(Of String, String)

        ''' <summary>
        ''' Remote client ip address
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Remote As String
        Public ReadOnly Property HttpRequest As HttpProcessor

        ''' <summary>
        ''' If current request url is indicates the HTTP root:  index.html
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsWWWRoot As Boolean
            Get
                Return String.Equals("/", URL)
            End Get
        End Property

        Dim m_cookies As Cookies

        ''' <summary>
        ''' Get from <see cref="URL"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Overridable ReadOnly Property Argument(name As String) As DefaultString
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New DefaultString(URL.query(name).ElementAtOrNull(Scan0))
            End Get
        End Property

        Sub New(request As HttpProcessor)
            HTTPMethod = request.http_method
            URL = New URL(request.http_url)
            version = request.http_protocol_versionstring
            HttpHeaders = request.httpHeaders
            Remote = request.socket.Client.RemoteEndPoint.ToString.Split(":"c).First
            HttpRequest = request
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' Debug use
        ''' </summary>
        ''' <param name="args"></param>
        Friend Sub New(args As IEnumerable(Of NamedValue(Of String)))
            HTTPMethod = "GET"
            URL = URL.BuildUrl("memory://debug", query:=args)
            version = "2.1"
            HttpHeaders = New Dictionary(Of String, String)
            Remote = "127.0.0.1"
        End Sub

        Public Overridable Function GetBoolean(name As String) As Boolean
            If URL.query.ContainsKey(name) Then
                Return URL.query(name).ElementAtOrDefault(Scan0).ParseBoolean
            Else
                Return False
            End If
        End Function

        Public Function GetCookies() As Cookies
            If m_cookies Is Nothing Then
                m_cookies = Cookies.ParseCookies(HttpHeaders.TryGetValue(RequestHeaders.Cookie))
            End If

            Return m_cookies
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetArguments() As Dictionary(Of String, Object)
            Return URL.query.ToDictionary(Function(a) a.Key, Function(a) CObj(a.Value))
        End Function

        Public Overridable Function HasValue(name As String) As Boolean
            Return URL.query.ContainsKey(name)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

End Namespace
