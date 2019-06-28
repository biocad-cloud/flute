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
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Core.WebSocket

    Public Class WebSocket
        Inherits System.Net.Sockets.TcpListener

        Delegate Sub OnClientConnectDelegate(ByVal sender As Object, ByRef Client As WsProcessor)
        Event OnClientConnect As OnClientConnectDelegate


        Dim WithEvents PendingCheckTimer As New Timers.Timer(500)
        Dim WithEvents ClientDataAvailableTimer As New Timers.Timer(50)
        Property ClientCollection As New List(Of WsProcessor)



        Sub New(ByVal url As String, ByVal port As Integer)
            MyBase.New(IPAddress.Parse(url), port)
        End Sub


        Sub startServer()
            Me.Start()
            PendingCheckTimer.Start()
        End Sub



        Sub Client_Connected(ByVal sender As Object, ByRef client As WsProcessor) Handles Me.OnClientConnect
            Me.ClientCollection.Add(client)
            AddHandler client.onClientDisconnect, AddressOf Client_Disconnected
            client.HandShake()
            ClientDataAvailableTimer.Start()
        End Sub


        Sub Client_Disconnected()

        End Sub


        Function isClientDisconnected(ByVal client As WsProcessor) As Boolean
            isClientDisconnected = False
            If Not client.isConnected Then
                Return True
            End If
        End Function


        Function isClientConnected(ByVal client As WsProcessor) As Boolean
            isClientConnected = False
            If client.isConnected Then
                Return True
            End If
        End Function


        Private Sub PendingCheckTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles PendingCheckTimer.Elapsed
            If Pending() Then
                RaiseEvent OnClientConnect(Me, New WsProcessor(Me.AcceptTcpClient()))
            End If
        End Sub


        Private Sub ClientDataAvailableTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ClientDataAvailableTimer.Elapsed
            Me.ClientCollection.RemoveAll(AddressOf isClientDisconnected)
            If Me.ClientCollection.Count < 1 Then ClientDataAvailableTimer.Stop()

            For Each Client As WsProcessor In Me.ClientCollection
                Client.CheckForDataAvailability()
            Next
        End Sub
    End Class
End Namespace
