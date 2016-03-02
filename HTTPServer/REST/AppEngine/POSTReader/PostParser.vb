Imports System.Text
Imports System.Collections
Imports System.Collections.Specialized
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Security.Principal
Imports System.Threading
Imports System.Web.Configuration
Imports System.Web.Management
Imports System.Web.UI
Imports System.Web.Util
Imports System.Globalization
Imports System.Security.Authentication.ExtendedProtection
Imports System.Web.Routing
Imports System.Web

''' <summary>
''' POST参数的解析工具
''' </summary>
Public Class PostParser

    Private Shared Function GetParameter(header As String, attr As String) As String
        Dim ap As Integer = header.IndexOf(attr)
        If ap = -1 Then
            Return Nothing
        End If

        ap += attr.Length
        If ap >= header.Length Then
            Return Nothing
        End If

        Dim ending As Char = header(ap)
        If ending <> """"c Then
            ending = " "c
        End If

        Dim [end] As Integer = header.IndexOf(ending, ap + 1)
        If [end] = -1 Then
            Return If((ending = """"c), Nothing, header.Substring(ap))
        End If

        Return header.Substring(ap + 1, [end] - ap - 1)
    End Function

    Public ReadOnly Property ContentType() As String
    Public ReadOnly Property InputStream() As MemoryStream
    Public ReadOnly Property ContentEncoding As Encoding

    Sub New(input As MemoryStream, contentType As String, encoding As Encoding)
        Me.InputStream = input
        Me.ContentType = contentType
        Me.ContentEncoding = encoding

        Call LoadMultiPart()
    End Sub

    ' GetSubStream returns a 'copy' of the InputStream with Position set to 0.
    Private Shared Function GetSubStream(stream As Stream) As Stream
        Dim other As MemoryStream = DirectCast(stream, MemoryStream)
        Return New MemoryStream(other.GetBuffer(), 0, CInt(other.Length), False, True)
    End Function

    ''' <summary>
    ''' Loads the data on the form for multipart/form-data
    ''' </summary>
    Private Sub LoadMultiPart()
        Dim boundary As String = GetParameter(ContentType, "; boundary=")
        If boundary Is Nothing Then
            Return
        End If

        Dim input As Stream = GetSubStream(InputStream)
        Dim multi_part As New HttpMultipart(input, boundary, ContentEncoding)

        Dim e As HttpMultipart.Element = Nothing
        While multi_part.ReadNextElement().ShadowCopy(e) IsNot Nothing
            If e.Filename Is Nothing Then
                Dim copy As Byte() = New Byte(e.Length - 1) {}

                input.Position = e.Start
                input.Read(copy, 0, CInt(e.Length))

                Form.Add(e.Name, ContentEncoding.GetString(copy))
            Else
                '
                ' We use a substream, as in 2.x we will support large uploads streamed to disk,
                '
                Dim [sub] As New HttpPostedFile(e.Filename, e.ContentType, input, e.Start, e.Length)
                Files.Add(e.Name, [sub])
            End If
        End While
    End Sub

    Public ReadOnly Property Form As New NameValueCollection
    Public ReadOnly Property Files As New Dictionary(Of String, HttpPostedFile)
End Class