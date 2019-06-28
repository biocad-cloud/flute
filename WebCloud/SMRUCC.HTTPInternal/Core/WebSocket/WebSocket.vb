#Region "Microsoft.VisualBasic::4bceaf76bb3058282e4f8f2758302ca3, WebCloud\SMRUCC.HTTPInternal\Core\WebSocket.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Class WebSocket
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.Sockets
Imports System.Timers

Namespace Core.WebSocket

    Public Class WsServer : Inherits TcpListener

        Public Event OnClientConnect As OnClientConnectDelegate

        Dim WithEvents PendingCheckTimer As New Timer(500)
        Dim WithEvents ClientDataAvailableTimer As New Timer(50)

        Dim ClientCollection As New List(Of WsProcessor)

        Sub New(url As String, port As Integer)
            MyBase.New(IPAddress.Parse(url), port)
        End Sub

        Sub StartServer()
            Me.Start()
            PendingCheckTimer.Start()
        End Sub

        Sub Client_Connected(sender As Object, ByRef client As WsProcessor) Handles Me.OnClientConnect
            Me.ClientCollection.Add(client)
            AddHandler client.onClientDisconnect, AddressOf Client_Disconnected
            client.HandShake()
            ClientDataAvailableTimer.Start()
        End Sub

        Sub Client_Disconnected()

        End Sub

        Function isClientDisconnected(client As WsProcessor) As Boolean
            isClientDisconnected = False
            If Not client.isConnected Then
                Return True
            End If
        End Function

        Function isClientConnected(client As WsProcessor) As Boolean
            isClientConnected = False
            If client.isConnected Then
                Return True
            End If
        End Function

        Private Sub PendingCheckTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles PendingCheckTimer.Elapsed
            If Pending() Then
                RaiseEvent OnClientConnect(Me, New WsProcessor(Me.AcceptTcpClient()))
            End If
        End Sub

        Private Sub ClientDataAvailableTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles ClientDataAvailableTimer.Elapsed
            Me.ClientCollection.RemoveAll(AddressOf isClientDisconnected)
            If Me.ClientCollection.Count < 1 Then ClientDataAvailableTimer.Stop()

            For Each Client As WsProcessor In Me.ClientCollection
                Client.CheckForDataAvailability()
            Next
        End Sub
    End Class
End Namespace
