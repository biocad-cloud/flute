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

Public Class Class1

    Friend Shared Function GetParameter(header As String, attr As String) As String
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

    Public Property ContentType() As String
    '    Get
    '        If content_type Is Nothing Then
    '            If worker_request IsNot Nothing Then
    '                content_type = worker_request.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType)
    '            End If

    '            If content_type Is Nothing Then
    '                content_type = [String].Empty
    '            End If
    '        End If

    '        Return content_type
    '    End Get

    '    Set
    '        content_type = Value
    '    End Set
    'End Property

    Public ReadOnly Property InputStream() As Stream

    ' GetSubStream returns a 'copy' of the InputStream with Position set to 0.
    Private Shared Function GetSubStream(stream As Stream) As Stream
        If TypeOf stream Is IntPtrStream Then
            Return New IntPtrStream(stream)
        End If

        If TypeOf stream Is MemoryStream Then
            Dim other As MemoryStream = DirectCast(stream, MemoryStream)
            Return New MemoryStream(other.GetBuffer(), 0, CInt(other.Length), False, True)
        End If

        If TypeOf stream Is TempFileStream Then
            DirectCast(stream, TempFileStream).SavePosition()
            Return stream
        End If

        Throw New NotSupportedException("The stream is " & Convert.ToString(stream.[GetType]()))
    End Function
    Public Property ContentEncoding() As Encoding
    '
    ' Loads the data on the form for multipart/form-data
    '
    Private Sub LoadMultiPart()
        Dim boundary As String = GetParameter(ContentType, "; boundary=")
        If boundary Is Nothing Then
            Return
        End If

        Dim input As Stream = GetSubStream(InputStream)
        Dim multi_part As New HttpMultipart(input, boundary, ContentEncoding)

        Dim e As HttpMultipart.Element
        While (InlineAssignHelper(e, multi_part.ReadNextElement())) IsNot Nothing
            If e.Filename Is Nothing Then
                Dim copy As Byte() = New Byte(e.Length - 1) {}

                input.Position = e.Start
                input.Read(copy, 0, CInt(e.Length))

                m_form.Add(e.Name, ContentEncoding.GetString(copy))
            Else
                '
                ' We use a substream, as in 2.x we will support large uploads streamed to disk,
                '
                Dim [sub] As New HttpPostedFile(e.Filename, e.ContentType, input, e.Start, e.Length)
                m_files.Add(e.Name, [sub])
            End If
        End While
        EndSubStream(input)
    End Sub

    Dim m_form As Specialized.NameValueCollection
    Dim m_files As Dictionary(Of String, HttpPostedFile)

    Private Shared Sub EndSubStream(stream As Stream)
        If TypeOf stream Is TempFileStream Then
            DirectCast(stream, TempFileStream).RestorePosition()
        End If
    End Sub

End Class


'
' Stream-based multipart handling.
'
' In this incarnation deals with an HttpInputStream as we are now using
' IntPtr-based streams instead of byte [].   In the future, we will also
' send uploads above a certain threshold into the disk (to implement
' limit-less HttpInputFiles). 
'

Public Class HttpMultipart

    Public Class Element
        Public ContentType As String
        Public Name As String
        Public Filename As String
        Public Start As Long
        Public Length As Long

        Public Overrides Function ToString() As String
            Return "ContentType " & ContentType & ", Name " & Name & ", Filename " & Filename & ", Start " & Start.ToString() & ", Length " & Length.ToString()
        End Function
    End Class

    Private data As Stream
    Private boundary As String
    Private boundary_bytes As Byte()
    Private buffer As Byte()
    Private at_eof As Boolean
    Private encoding As Encoding
    Private sb As StringBuilder

    Const HYPHEN As Byte = CByte(AscW("-"c)), LF As Byte = CByte(AscW(ControlChars.Lf)), CR As Byte = CByte(AscW(ControlChars.Cr))

    ' See RFC 2046 
    ' In the case of multipart entities, in which one or more different
    ' sets of data are combined in a single body, a "multipart" media type
    ' field must appear in the entity's header.  The body must then contain
    ' one or more body parts, each preceded by a boundary delimiter line,
    ' and the last one followed by a closing boundary delimiter line.
    ' After its boundary delimiter line, each body part then consists of a
    ' header area, a blank line, and a body area.  Thus a body part is
    ' similar to an RFC 822 message in syntax, but different in meaning.

    Public Sub New(data As Stream, b As String, encoding As Encoding)
        Me.data = data
        boundary = b
        boundary_bytes = encoding.GetBytes(b)
        buffer = New Byte(boundary_bytes.Length + 1) {}
        ' CRLF or '--'
        Me.encoding = encoding
        sb = New StringBuilder()
    End Sub

    Private Function ReadLine() As String
        ' CRLF or LF are ok as line endings.
        Dim got_cr As Boolean = False
        Dim b As Integer = 0
        sb.Length = 0
        While True
            b = data.ReadByte()
            If b = -1 Then
                Return Nothing
            End If

            If b = LF Then
                Exit While
            End If
            got_cr = (b = CR)
            sb.Append(ChrW(b))
        End While

        If got_cr Then
            sb.Length -= 1
        End If

        Return sb.ToString()

    End Function

    Private Shared Function GetContentDispositionAttribute(l As String, name As String) As String
        Dim idx As Integer = l.IndexOf(name & "=""")
        If idx < 0 Then
            Return Nothing
        End If
        Dim begin As Integer = idx + name.Length + "=""".Length
        Dim [end] As Integer = l.IndexOf(""""c, begin)
        If [end] < 0 Then
            Return Nothing
        End If
        If begin = [end] Then
            Return ""
        End If
        Return l.Substring(begin, [end] - begin)
    End Function

    Private Function GetContentDispositionAttributeWithEncoding(l As String, name As String) As String
        Dim idx As Integer = l.IndexOf(name & "=""")
        If idx < 0 Then
            Return Nothing
        End If
        Dim begin As Integer = idx + name.Length + "=""".Length
        Dim [end] As Integer = l.IndexOf(""""c, begin)
        If [end] < 0 Then
            Return Nothing
        End If
        If begin = [end] Then
            Return ""
        End If

        Dim temp As String = l.Substring(begin, [end] - begin)
        Dim source As Byte() = New Byte(temp.Length - 1) {}
        For i As Integer = temp.Length - 1 To 0 Step -1
            source(i) = CByte(AscW(temp(i)))
        Next

        Return encoding.GetString(source)
    End Function

    Private Function ReadBoundary() As Boolean
        Try
            Dim line As String = ReadLine()
            While line = ""
                line = ReadLine()
            End While
            If line(0) <> "-"c OrElse line(1) <> "-"c Then
                Return False
            End If

            If Not StrUtils.EndsWith(line, boundary, False) Then
                Return True
            End If
        Catch
        End Try

        Return False
    End Function

    Private Function ReadHeaders() As String
        Dim s As String = ReadLine()
        If s = "" Then
            Return Nothing
        End If

        Return s
    End Function

    Private Function CompareBytes(orig As Byte(), other As Byte()) As Boolean
        For i As Integer = orig.Length - 1 To 0 Step -1
            If orig(i) <> other(i) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function MoveToNextBoundary() As Long
        Dim retval As Long = 0
        Dim got_cr As Boolean = False

        Dim state As Integer = 0
        Dim c As Integer = data.ReadByte()
        While True
            If c = -1 Then
                Return -1
            End If

            If state = 0 AndAlso c = LF Then
                retval = data.Position - 1
                If got_cr Then
                    retval -= 1
                End If
                state = 1
                c = data.ReadByte()
            ElseIf state = 0 Then
                got_cr = (c = CR)
                c = data.ReadByte()
            ElseIf state = 1 AndAlso c = "-"c Then
                c = data.ReadByte()
                If c = -1 Then
                    Return -1
                End If

                If c <> "-"c Then
                    state = 0
                    got_cr = False
                    ' no ReadByte() here
                    Continue While
                End If

                Dim nread As Integer = data.Read(buffer, 0, buffer.Length)
                Dim bl As Integer = buffer.Length
                If nread <> bl Then
                    Return -1
                End If

                If Not CompareBytes(boundary_bytes, buffer) Then
                    state = 0
                    data.Position = retval + 2
                    If got_cr Then
                        data.Position += 1
                        got_cr = False
                    End If
                    c = data.ReadByte()
                    Continue While
                End If

                If buffer(bl - 2) = "-"c AndAlso buffer(bl - 1) = "-"c Then
                    at_eof = True
                ElseIf buffer(bl - 2) <> CR OrElse buffer(bl - 1) <> LF Then
                    state = 0
                    data.Position = retval + 2
                    If got_cr Then
                        data.Position += 1
                        got_cr = False
                    End If
                    c = data.ReadByte()
                    Continue While
                End If
                data.Position = retval + 2
                If got_cr Then
                    data.Position += 1
                End If
                Exit While
            Else
                ' state == 1
                ' no ReadByte() here
                state = 0
            End If
        End While

        Return retval
    End Function

    Public Function ReadNextElement() As Element
        If at_eof OrElse ReadBoundary() Then
            Return Nothing
        End If

        Dim elem As New Element()
        Dim header As String = Nothing
        While (InlineAssignHelper(header, ReadHeaders())) IsNot Nothing
            If StrUtils.StartsWith(header, "Content-Disposition:", True) Then
                elem.Name = GetContentDispositionAttribute(header, "name")
                elem.Filename = StripPath(GetContentDispositionAttributeWithEncoding(header, "filename"))
            ElseIf StrUtils.StartsWith(header, "Content-Type:", True) Then
                elem.ContentType = header.Substring("Content-Type:".Length).Trim()
            End If
        End While

        Dim start As Long = data.Position
        elem.Start = start
        Dim pos As Long = MoveToNextBoundary()
        If pos = -1 Then
            Return Nothing
        End If

        elem.Length = pos - start
        Return elem
    End Function

    Private Shared Function StripPath(path As String) As String
        If path Is Nothing OrElse path.Length = 0 Then
            Return path
        End If

        If path.IndexOf(":\") <> 1 AndAlso Not path.StartsWith("\\") Then
            Return path
        End If
        Return path.Substring(path.LastIndexOf("\"c) + 1)
    End Function
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class