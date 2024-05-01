Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

Public Class SessionFile

    ReadOnly file As String

    Sub New(filepath As String)
        file = filepath
    End Sub

    Public Function OpenKey(key As String) As MemoryStream
        Dim region As BufferRegion = SearchKey(key)

        If region Is Nothing Then
            Return Nothing
        Else

        End If
    End Function

    Public Function SearchKey(key As String) As BufferRegion

    End Function

End Class
