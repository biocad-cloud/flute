Imports System.IO

Namespace AppEngine.POSTParser

    Public Class ReadSubStream
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
End Namespace