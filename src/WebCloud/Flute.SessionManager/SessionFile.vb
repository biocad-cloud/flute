Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO

Public Class SessionFile

    ReadOnly file As String

    Sub New(filepath As String)
        file = filepath

        If Not file.FileExists Then
            Call New Byte() {}.FlushStream(file)
        End If
    End Sub

    Public Function OpenKey(key As String) As MemoryStream
        Dim region As BufferRegion = SearchKey(key)

        If region Is Nothing Then
            Return Nothing
        Else
            Using s As New FileStream(file, FileMode.Open)
                Dim load As Byte() = New Byte(region.size - 1) {}

                Call s.Seek(region.position, SeekOrigin.Begin)
                Call s.Read(load, Scan0, load.Length)

                Return New MemoryStream(load)
            End Using
        End If
    End Function

    ''' <summary>
    ''' [keyname => offset,length,next]
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Public Function SearchKey(key As String) As BufferRegion
        Using s As New BinaryDataReader(New FileStream(file, FileMode.Open), Encoding.ASCII)
            For i As Integer = 0 To 100000
                Dim skey As String = s.ReadString(BinaryStringFormat.ZeroTerminated)
                Dim start As Long = s.ReadInt64
                Dim len As Integer = s.ReadInt32

                If skey = key Then
                    Return New BufferRegion(start, Len)
                Else
                    Dim jumpNext As Long = s.ReadInt64

                    Call s.Seek(jumpNext, SeekOrigin.Begin)

                    If s.EndOfStream Then
                        Exit For
                    End If
                End If
            Next

            Return Nothing
        End Using
    End Function

End Class
