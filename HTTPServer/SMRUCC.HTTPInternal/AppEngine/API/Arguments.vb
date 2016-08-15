Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
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

        '
        ' Summary:
        '     Closes the current StreamWriter object and the underlying stream.
        '
        ' Exceptions:
        '   T:System.Text.EncoderFallbackException:
        '     The current encoding does not support displaying half of a Unicode surrogate
        '     pair.
        Public Sub Close()
            Call response.Close()
            Call response.Dispose()
        End Sub

        '
        ' Summary:
        '     Clears all buffers for the current writer and causes any buffered data to be
        '     written to the underlying stream.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The current writer is closed.
        '
        '   T:System.IO.IOException:
        '     An I/O error has occurred.
        '
        '   T:System.Text.EncoderFallbackException:
        '     The current encoding does not support displaying half of a Unicode surrogate
        '     pair.
        Public Sub Flush()
            Call response.Flush()
        End Sub
        '

        ' Summary:
        '     Writes a character to the stream.
        '
        ' Parameters:
        '   value:
        '     The character to write to the stream.
        '
        ' Exceptions:
        '   T:System.IO.IOException:
        '     An I/O error occurs.
        '
        '   T:System.ObjectDisposedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and current writer is closed.
        '
        '   T:System.NotSupportedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and the contents of the buffer cannot be written to the underlying fixed
        '     size stream because the System.IO.StreamWriter is at the end the stream.
        Public Sub Write(value As Char)
            Call response.Write(value)
        End Sub
        '
        ' Summary:
        '     Writes a string to the stream.
        '
        ' Parameters:
        '   value:
        '     The string to write to the stream. If value is null, nothing is written.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and current writer is closed.
        '
        '   T:System.NotSupportedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and the contents of the buffer cannot be written to the underlying fixed
        '     size stream because the System.IO.StreamWriter is at the end the stream.
        '
        '   T:System.IO.IOException:
        '     An I/O error occurs.
        Public Sub Write(value As String)
            Call response.Write(value)
        End Sub
        ' Summary:
        '     Writes a character array to the stream.
        '
        ' Parameters:
        '   buffer:
        '     A character array containing the data to write. If buffer is null, nothing is
        '     written.
        '
        ' Exceptions:
        '   T:System.IO.IOException:
        '     An I/O error occurs.
        '
        '   T:System.ObjectDisposedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and current writer is closed.
        '
        '   T:System.NotSupportedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and the contents of the buffer cannot be written to the underlying fixed
        '     size stream because the System.IO.StreamWriter is at the end the stream.
        Public Sub Write(buffer() As Char)
            Call response.Write(buffer)
        End Sub
        ' Summary:
        '     Writes a subarray of characters to the stream.
        '
        ' Parameters:
        '   buffer:
        '     A character array that contains the data to write.
        '
        '   index:
        '     The character position in the buffer at which to start reading data.
        '
        '   count:
        '     The maximum number of characters to write.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     buffer is null.
        '
        '   T:System.ArgumentException:
        '     The buffer length minus index is less than count.
        '
        '   T:System.ArgumentOutOfRangeException:
        '     index or count is negative.
        '
        '   T:System.IO.IOException:
        '     An I/O error occurs.
        '
        '   T:System.ObjectDisposedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and current writer is closed.
        '
        '   T:System.NotSupportedException:
        '     System.IO.StreamWriter.AutoFlush is true or the System.IO.StreamWriter buffer
        '     is full, and the contents of the buffer cannot be written to the underlying fixed
        '     size stream because the System.IO.StreamWriter is at the end the stream.
        Public Sub Write(buffer() As Char, index As Integer, count As Integer)
            Call response.Write(buffer, index, count)
        End Sub

        '
        ' Summary:
        '     Clears all buffers for this stream asynchronously and causes any buffered data
        '     to be written to the underlying device.
        '
        ' Returns:
        '     A task that represents the asynchronous flush operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream has been disposed.
        <ComVisible(False)>
        Public Function FlushAsync() As Tasks.Task
            Return response.FlushAsync
        End Function
        '
        ' Summary:
        '     Writes a character to the stream asynchronously.
        '
        ' Parameters:
        '   value:
        '     The character to write to the stream.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteAsync(value As Char) As Tasks.Task
            Return response.WriteAsync(value)
        End Function
        '
        ' Summary:
        '     Writes a string to the stream asynchronously.
        '
        ' Parameters:
        '   value:
        '     The string to write to the stream. If value is null, nothing is written.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteAsync(value As String) As Tasks.Task
            Return response.WriteAsync(value)
        End Function
        '
        ' Summary:
        '     Writes a subarray of characters to the stream asynchronously.
        '
        ' Parameters:
        '   buffer:
        '     A character array that contains the data to write.
        '
        '   index:
        '     The character position in the buffer at which to begin reading data.
        '
        '   count:
        '     The maximum number of characters to write.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     buffer is null.
        '
        '   T:System.ArgumentException:
        '     The index plus count is greater than the buffer length.
        '
        '   T:System.ArgumentOutOfRangeException:
        '     index or count is negative.
        '
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteAsync(buffer() As Char, index As Integer, count As Integer) As Tasks.Task
            Return response.WriteAsync(buffer, index, count)
        End Function
        '
        ' Summary:
        '     Writes a line terminator asynchronously to the stream.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteLineAsync() As Tasks.Task
            Return response.WriteLineAsync
        End Function
        '
        ' Summary:
        '     Writes a character followed by a line terminator asynchronously to the stream.
        '
        ' Parameters:
        '   value:
        '     The character to write to the stream.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteLineAsync(value As Char) As Tasks.Task
            Return response.WriteLineAsync(value)
        End Function
        '
        ' Summary:
        '     Writes a string followed by a line terminator asynchronously to the stream.
        '
        ' Parameters:
        '   value:
        '     The string to write. If the value is null, only a line terminator is written.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteLineAsync(value As String) As Tasks.Task
            Return response.WriteLineAsync(value)
        End Function
        '
        ' Summary:
        '     Writes a subarray of characters followed by a line terminator asynchronously
        '     to the stream.
        '
        ' Parameters:
        '   buffer:
        '     The character array to write data from.
        '
        '   index:
        '     The character position in the buffer at which to start reading data.
        '
        '   count:
        '     The maximum number of characters to write.
        '
        ' Returns:
        '     A task that represents the asynchronous write operation.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     buffer is null.
        '
        '   T:System.ArgumentException:
        '     The index plus count is greater than the buffer length.
        '
        '   T:System.ArgumentOutOfRangeException:
        '     index or count is negative.
        '
        '   T:System.ObjectDisposedException:
        '     The stream writer is disposed.
        '
        '   T:System.InvalidOperationException:
        '     The stream writer is currently in use by a previous write operation.
        <ComVisible(False)>
        Public Function WriteLineAsync(buffer() As Char, index As Integer, count As Integer) As Tasks.Task
            Return response.WriteLineAsync(buffer, index, count)
        End Function


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