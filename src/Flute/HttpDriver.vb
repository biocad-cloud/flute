﻿Imports System.Runtime.CompilerServices
Imports Flute.Http.Configurations
Imports Flute.Http.Core
Imports Flute.Http.Core.Message

Public Class HttpDriver

    Dim responseHeader As New Dictionary(Of String, String)
    Dim methods As New Dictionary(Of String, HttpSocket.AppHandler)
    Dim settings As Configuration

    Sub New(Optional settings As Configuration = Nothing)
        Me.settings = settings
    End Sub

    Public Sub HttpMethod(method As String, handler As HttpSocket.AppHandler)
        methods(method.ToUpper) = handler
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddResponseHeader(header As String, value As String)
        Call responseHeader.Add(header, value)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetSocket(port As Integer) As HttpSocket
        Return New HttpSocket(
            app:=AddressOf AppHandler,
            port:=port,
            configs:=settings
        )
    End Function

    Public Sub AppHandler(request As HttpRequest, response As HttpResponse)
        For Each header In responseHeader
            response.AddCustomHttpHeader(header.Key, header.Value)
        Next

        If methods.ContainsKey(request.HTTPMethod) Then
            Call methods(request.HTTPMethod)(request, response)
        Else
            Call response.WriteError(501, "501 Not Implemented")
        End If
    End Sub

End Class