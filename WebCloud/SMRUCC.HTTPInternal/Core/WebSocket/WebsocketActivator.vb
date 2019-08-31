Imports System.Net.Sockets

Namespace Core.WebSocket

    Public Delegate Function WebsocketActivator(tcp As TcpClient) As WsProcessor

End Namespace