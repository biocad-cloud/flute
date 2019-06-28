Imports SMRUCC.WebCloud.HTTPInternal.Core

Module Module1

    Sub Main()  'Program Entry point
        Dim thread As System.Threading.Thread = New System.Threading.Thread(AddressOf StartWebSocketServer)
        'Application.Add("WebSocketServerThread", thread) 'Global.asax - context.Application .. I left this part in for web application developers  
        thread.Start()
    End Sub

    Public WebSocketServer As WebSocket
    Public Sub StartWebSocketServer()
        WebSocketServer = New WebSocket("127.0.0.1", 8000)
        WebSocketServer.startServer()
    End Sub

End Module
