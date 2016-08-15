Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Terminal.STDIO__
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

    Public Class HttpResponse
        Implements IDisposable

        ReadOnly response As StreamWriter

        Sub New(rep As StreamWriter)
            response = rep
        End Sub

        Public Sub Redirect(url As String)
            Call WriteHTML(<script>window.location='%s';</script>, url)
        End Sub

        Public Sub WriteHTML(html As String)
            Call response.WriteLine(html)
        End Sub

        ''' <summary>
        ''' %s %d, etc
        ''' </summary>
        ''' <param name="html">C language like printf function format usage.</param>
        ''' <param name="args"></param>
        Public Sub WriteHTML(html As XElement, ParamArray args As Object())
            Call WriteHTML(sprintf(html.ToString, args))
        End Sub

        Public Sub WriteJSON(Of T)(obj As T)
            Call WriteHTML(obj.GetJson)
        End Sub

        Public Sub WriteXML(Of T)(obj As T)
            Call WriteHTML(obj.GetXml)
        End Sub

        Public Shared Operator <=(rep As HttpResponse, url As String) As HttpResponse
            Call rep.Redirect(url)
            Return rep
        End Operator

        Public Shared Operator >=(rep As HttpResponse, url As String) As HttpResponse
            Throw New NotSupportedException
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace