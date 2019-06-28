Imports SMRUCC.WebCloud.HTTPInternal.Core

Module Module1

    Sub Main()  'Program Entry point
        Call StartWebSocketServer()


        Pause()
    End Sub

    Public WebSocketServer As WebSocket
    Public Sub StartWebSocketServer()
        WebSocketServer = New WebSocket("127.0.0.1", 8000)
        WebSocketServer.startServer()
    End Sub

End Module
