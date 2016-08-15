Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.HTTPInternal.AppEngine.POSTParser
Imports SMRUCC.HTTPInternal.Core
Imports SMRUCC.HTTPInternal.Platform

Namespace AppEngine.APIMethods.Arguments

    ''' <summary>
    ''' Data of the http request
    ''' </summary>
    Public Class HttpRequest

        ''' <summary>
        ''' GET/POST/PUT/DELETE....
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HTTPMethod As String
        Public ReadOnly Property URL As String
        ''' <summary>
        ''' <see cref="HttpProcessor.http_protocol_versionstring"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property version As String
        Public ReadOnly Property HttpHeaders As Dictionary(Of String, String)

        ''' <summary>
        ''' If current request url is indicates the HTTP root:  index.html
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsWWWRoot As Boolean
            Get
                Return String.Equals("/", URL)
            End Get
        End Property

        Sub New(request As HttpProcessor)
            HTTPMethod = request.http_method
            URL = request.http_url
            version = request.http_protocol_versionstring
            HttpHeaders = request.httpHeaders
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class HttpPOSTRequest : Inherits HttpRequest

        Public ReadOnly Property POSTData As PostReader

        Sub New(request As HttpProcessor, inputData As MemoryStream)
            Call MyBase.New(request)
            POSTData = New PostReader(inputData, HttpHeaders(PlatformEngine.contentType), Encoding.UTF8)
        End Sub
    End Class
End Namespace