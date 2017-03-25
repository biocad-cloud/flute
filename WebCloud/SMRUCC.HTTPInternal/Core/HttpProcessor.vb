﻿#Region "Microsoft.VisualBasic::fb4fd661fdb512ce527449b82aa226b7, ..\httpd\WebCloud\SMRUCC.HTTPInternal\Core\HttpProcessor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

' offered to the public domain for any use with no restriction
' and also with no warranty of any kind, please enjoy. - David Jeske. 

' simple HTTP explanation
' http://www.jmarshall.com/easy/http/

Namespace Core

    ''' <summary>
    ''' 这个对象包含有具体的http request的处理方法
    ''' </summary>
    Public Class HttpProcessor : Implements IDisposable

        Public socket As TcpClient
        Public srv As HttpServer

        Dim _inputStream As Stream

        Public outputStream As StreamWriter

        Public Property http_method As String
        ''' <summary>
        ''' File location or GET/POST request arguments
        ''' </summary>
        ''' <returns></returns>
        Public Property http_url As String
        Public Property http_protocol_versionstring As String
        Public Property httpHeaders As New Dictionary(Of String, String)

        ''' <summary>
        ''' 可以向这里面写入数据从而回传数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Out As Stream
            Get
                Return outputStream.BaseStream
            End Get
        End Property

        ''' <summary>
        ''' 10MB
        ''' </summary>
        ''' <remarks></remarks>
        Const MAX_POST_SIZE As Integer = 128 * 1024 * 1024

        Public Sub New(s As TcpClient, srv As HttpServer)
            Me.socket = s
            Me.srv = srv
        End Sub

        ''' <summary>
        ''' If current request url is indicates the HTTP root:  index.html
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsWWWRoot As Boolean
            Get
                Return String.Equals("/", http_url)
            End Get
        End Property

        Public Sub WriteData(data As Byte())
            Call outputStream.BaseStream.Write(data, Scan0, data.Length)
        End Sub

        Public Sub WriteLine(s As String)
            Call outputStream.WriteLine(s)
        End Sub

        Public Overrides Function ToString() As String
            Return http_url
        End Function

        Private Function __streamReadLine(inputStream As Stream) As String
            Dim nextChar As Integer
            Dim chrbuf As New List(Of Char)
            Dim n As Integer

            While True

                nextChar = inputStream.ReadByte()

                If nextChar = ASCII.Byte.LF Then
                    Exit While
                End If
                If nextChar = ASCII.Byte.CR Then
                    Continue While
                End If
                If nextChar = -1 Then
                    Call Thread.Sleep(1)
                    n += 1
                    If n > 1024 Then
                        Exit While
                    Else
                        Continue While
                    End If
                End If

                Call chrbuf.Add(Convert.ToChar(nextChar))
            End While

            Return New String(chrbuf.ToArray)
        End Function

        Public Sub Process()
            ' we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            ' "processed" view of the world, and we want the data raw after the headers
            _inputStream = New BufferedStream(socket.GetStream())

            ' we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = New StreamWriter(New BufferedStream(socket.GetStream()))
            Try
                Call __processInvoker()
            Catch e As Exception
                Call e.PrintException
                writeFailure(e.ToString)
            End Try

            Try
                Call outputStream.Flush()
            Catch ex As Exception
                Call App.LogException(ex)
            Finally
                Try
                    Call outputStream.Close()
                    Call outputStream.Dispose()
                Catch ex As Exception
                    Call App.LogException(ex)
                End Try
            End Try

            ' bs.Flush(); // flush any remaining output
            _inputStream = Nothing
            outputStream = Nothing

            Try
                Call socket.Close()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub __processInvoker()
            Call parseRequest()
            Call readHeaders()

            If http_method.Equals("GET", StringComparison.OrdinalIgnoreCase) Then
                handleGETRequest()

            ElseIf http_method.Equals("POST", StringComparison.OrdinalIgnoreCase) Then
                HandlePOSTRequest()

            Else
                ' Dim msg As String = $"Unsupport {NameOf(http_method)}:={http_method}"
                ' Call msg.__DEBUG_ECHO
                ' Call writeFailure(msg)
                Call srv.handleOtherMethod(Me)
            End If
        End Sub

        Public Sub parseRequest()
            Dim request As String = __streamReadLine(_inputStream)

            If request.StringEmpty Then
                Dim wait% = 10

                Do While request.StringEmpty
                    ' 可能是网络传输速度比较慢，在这里等待一段时间再解析流之中的数据
                    ' 但是当前的这条处理线程最多只等待wait次数
                    Call Thread.Sleep(5)

                    If wait <= 0 Then
                        Exit Do
                    Else
                        request = __streamReadLine(_inputStream)
                        wait -= 1
                    End If
                Loop
            End If

            Dim tokens As String() = request.Split(" "c)

            If tokens.Length <> 3 Then
                Throw New Exception("invalid http request line: " & request)
            End If

            http_method = tokens(0).ToUpper()
            http_url = tokens(1)
            http_protocol_versionstring = tokens(2)

            Call $"starting: {request}".__INFO_ECHO
        End Sub

        Public Sub readHeaders()
            Dim line As String = "", s As New Value(Of String)

            Call NameOf(readHeaders).__DEBUG_ECHO

            While (s = __streamReadLine(_inputStream)) IsNot Nothing
                If s.value.StringEmpty Then
                    Call "got headers".__DEBUG_ECHO
                    Return
                Else
                    line = s.value
                End If

                Dim separator As Integer = line.IndexOf(":"c)
                If separator = -1 Then
                    Throw New Exception("invalid http header line: " & line)
                End If
                Dim name As String = line.Substring(0, separator)
                Dim pos As Integer = separator + 1
                While (pos < line.Length) AndAlso (line(pos) = " "c)
                    ' strip any spaces
                    pos += 1
                End While

                Dim value As String = line.Substring(pos, line.Length - pos)
                Call $"header: {name}:{value}".__DEBUG_ECHO
                httpHeaders(name) = value
            End While
        End Sub

        Public Sub handleGETRequest()
            Call srv.handleGETRequest(Me)
        End Sub

        Public BUF_SIZE As Integer = 4096

        Public Const ContentLengthTooLarge As String = "POST Content-Length({0}) too big for this simple server"
        Public Const ContentLength As String = "Content-Length"

        ''' <summary>
        ''' This post data processing just reads everything into a memory stream.
        ''' this is fine for smallish things, but for large stuff we should really
        ''' hand an input stream to the request processor. However, the input stream 
        ''' we hand him needs to let him see the "end of the stream" at this content 
        ''' length, because otherwise he won't know when he's seen it all! 
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub HandlePOSTRequest()

            ' Call Console.WriteLine("get post data start")

            Dim content_len As Integer = 0
            Dim ms As New MemoryStream()

            If Me.httpHeaders.ContainsKey(ContentLength) Then
                content_len = Convert.ToInt32(Me.httpHeaders(ContentLength))
                If content_len > MAX_POST_SIZE Then
                    Throw New Exception(String.Format(ContentLengthTooLarge, content_len))
                End If
                Dim buf As Byte() = New Byte(BUF_SIZE - 1) {}
                Dim to_read As Integer = content_len
                While to_read > 0
                    ' Console.WriteLine("starting Read, to_read={0}", to_read)

                    Dim numread As Integer = Me._inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read))
                    ' Console.WriteLine("read finished, numread={0}", numread)
                    If numread = 0 Then
                        If to_read = 0 Then
                            Exit While
                        Else
                            Throw New Exception("client disconnected during post")
                        End If
                    End If
                    to_read -= numread
                    ms.Write(buf, 0, numread)
                End While

                Call ms.Seek(Scan0, SeekOrigin.Begin)
            End If

            ' Call Console.WriteLine("get post data end")
            Call srv.handlePOSTRequest(Me, ms)
        End Sub

        Public Sub writeSuccess(len&, Optional content_type As String = "text/html")
            Try
                Call __writeSuccess(
                    content_type, New Content With {
                        .Length = len
                    })
            Catch ex As Exception
                Call App.LogException(ex)
            End Try
        End Sub

        Private Sub __writeSuccess(content_type As String, content As Content)
            ' this is the successful HTTP response line
            outputStream.WriteLine("HTTP/1.0 200 OK")
            ' these are the HTTP headers...          
            outputStream.WriteLine("Content-Length: " & content.Length)
            outputStream.WriteLine("Content-Type: " & content_type)
            outputStream.WriteLine("Connection: close")
            ' ..add your own headers here if you like

            ' Call content.WriteHeader(outputStream)

            outputStream.WriteLine("X-Powered-By: Microsoft VisualBasic")
            outputStream.WriteLine("")
            ' this terminates the HTTP headers.. everything after this is HTTP body..
            outputStream.Flush()
        End Sub

        Public Sub writeSuccess(content As Content)
            Try
                Call __writeSuccess(content.Type, content)
            Catch ex As Exception
                ex = New Exception(content.GetJson)
                Call App.LogException(ex)
            End Try
        End Sub

        ''' <summary>
        ''' You can customize your 404 error page at here.
        ''' </summary>
        ''' <returns></returns>
        Public Property _404Page As String

        ''' <summary>
        ''' 404
        ''' </summary>
        Public Sub writeFailure(ex As String)
            Try
                Call __writeFailure(ex)
            Catch e As Exception
                Call App.LogException(e)
            End Try
        End Sub

        ''' <summary>
        ''' 404
        ''' </summary>
        Private Sub __writeFailure(ex As String)
            ' this is an http 404 failure response
            Call outputStream.WriteLine("HTTP/1.0 404 Not Found")
            ' these are the HTTP headers
            '   Call outputStream.WriteLine("Connection: close")
            ' ..add your own headers here

            Dim _404 As String

            If String.IsNullOrEmpty(_404Page) Then
                _404 = ex
            Else
                ' 404 page html file usually located in the root directory of the site, 
                ' If the Then file exists the read the page And replace the 
                ' Exception message With the Placeholder %Exception%

                _404 = Me._404Page.Replace("%EXCEPTION%", ex)
            End If

            Call outputStream.WriteLine(_404)
            Call outputStream.WriteLine("")         ' this terminates the HTTP headers.
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call outputStream.Flush()
                    Call outputStream.Close()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
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
