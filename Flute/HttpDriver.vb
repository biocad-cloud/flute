Imports System.Net.Sockets
Imports Flute.Http.Core

Public Class HttpDriver

    Dim responseHeader As New Dictionary(Of String, String)

    Sub New()
    End Sub

    Public Sub AddResponseHeader(header As String, value As String)
        responseHeader.Add(header, value)
    End Sub

    Public Function GetSocket(port As Integer) As HttpSocket

    End Function

End Class
