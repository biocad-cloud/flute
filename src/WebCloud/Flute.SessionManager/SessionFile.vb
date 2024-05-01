Imports System.IO
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

        End If
    End Function

    ''' <summary>
    ''' [keyname => offset,length,next]
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Public Function SearchKey(key As String) As BufferRegion
        Using s As New BinaryDataReader(New FileStream(file, FileMode.Open))

        End Using
    End Function

End Class
