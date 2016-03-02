'
' System.Web.HttpPostedFile.cs
'
' Author:
'	Dick Porter  <dick@ximian.com>
'      Ben Maurer   <benm@ximian.com>
'      Miguel de Icaza <miguel@novell.com>
'
' Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'

Imports System.IO
Imports System.Security.Permissions

Public NotInheritable Class HttpPostedFile
    Private name As String
    Private content_type As String
    Private stream As Stream

    Private Class ReadSubStream
        Inherits Stream
        Private s As Stream
        Private offset As Long
        Private [end] As Long
        Private m_position As Long

        Public Sub New(s As Stream, offset As Long, length As Long)
            Me.s = s
            Me.offset = offset
            Me.[end] = offset + length
            m_position = offset
        End Sub

        Public Overrides Sub Flush()
        End Sub

        Public Overrides Function Read(buffer As Byte(), dest_offset As Integer, count As Integer) As Integer
            If buffer Is Nothing Then
                Throw New ArgumentNullException("buffer")
            End If

            If dest_offset < 0 Then
                Throw New ArgumentOutOfRangeException("dest_offset", "< 0")
            End If

            If count < 0 Then
                Throw New ArgumentOutOfRangeException("count", "< 0")
            End If

            Dim len As Integer = buffer.Length
            If dest_offset > len Then
                Throw New ArgumentException("destination offset is beyond array size")
            End If
            ' reordered to avoid possible integer overflow
            If dest_offset > len - count Then
                Throw New ArgumentException("Reading would overrun buffer")
            End If

            If count > [end] - m_position Then
                count = CInt([end] - m_position)
            End If

            If count <= 0 Then
                Return 0
            End If

            s.Position = m_position
            Dim result As Integer = s.Read(buffer, dest_offset, count)
            If result > 0 Then
                m_position += result
            Else
                m_position = [end]
            End If

            Return result
        End Function

        Public Overrides Function ReadByte() As Integer
            If m_position >= [end] Then
                Return -1
            End If

            s.Position = m_position
            Dim result As Integer = s.ReadByte()
            If result < 0 Then
                m_position = [end]
            Else
                m_position += 1
            End If

            Return result
        End Function

        Public Overrides Function Seek(d As Long, origin As SeekOrigin) As Long
            Dim real As Long
            Select Case origin
                Case SeekOrigin.Begin
                    real = offset + d
                    Exit Select
                Case SeekOrigin.[End]
                    real = [end] + d
                    Exit Select
                Case SeekOrigin.Current
                    real = m_position + d
                    Exit Select
                Case Else
                    Throw New ArgumentException()
            End Select

            Dim virt As Long = real - offset
            If virt < 0 OrElse virt > Length Then
                Throw New ArgumentException()
            End If

            m_position = s.Seek(real, SeekOrigin.Begin)
            Return m_position
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotSupportedException()
        End Sub

        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length() As Long
            Get
                Return [end] - offset
            End Get
        End Property

        Public Overrides Property Position() As Long
            Get
                Return m_position - offset
            End Get
            Set
                If Value > Length Then
                    Throw New ArgumentOutOfRangeException()
                End If

                m_position = Seek(Value, SeekOrigin.Begin)
            End Set
        End Property
    End Class

    Public Sub New(name As String, content_type As String, base_stream As Stream, offset As Long, length As Long)
        Me.name = name
        Me.content_type = content_type
        Me.stream = New ReadSubStream(base_stream, offset, length)
    End Sub

    Public ReadOnly Property ContentType() As String
        Get
            Return (content_type)
        End Get
    End Property

    Public ReadOnly Property ContentLength() As Integer
        Get
            Return CInt(stream.Length)
        End Get
    End Property

    Public ReadOnly Property FileName() As String
        Get
            Return (name)
        End Get
    End Property

    Public ReadOnly Property InputStream() As Stream
        Get
            Return (stream)
        End Get
    End Property

    Public Sub SaveAs(filename As String)
        Dim buffer As Byte() = New Byte(16 * 1024 - 1) {}
        Dim old_post As Long = stream.Position

        Try
            File.Delete(filename)
            Using fs As FileStream = File.Create(filename)
                stream.Position = 0
                Dim n As Integer

                While (InlineAssignHelper(n, stream.Read(buffer, 0, 16 * 1024))) <> 0
                    fs.Write(buffer, 0, n)
                End While
            End Using
        Finally
            stream.Position = old_post
        End Try
    End Sub
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class