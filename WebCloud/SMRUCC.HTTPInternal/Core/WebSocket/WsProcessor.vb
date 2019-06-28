
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Core.WebSocket
    ' https://developer.mozilla.org/zh-CN/docs/Web/API/WebSockets_API/WebSocket_Server_Vb.NET

    ''' <summary>
    ''' A websocket client
    ''' </summary>
    Public Class WsProcessor
        Dim _TcpClient As TcpClient

        Public Delegate Sub OnClientDisconnectDelegateHandler()
        Public Event onClientDisconnect As OnClientDisconnectDelegateHandler

        Public ReadOnly Property isConnected As Boolean
            Get
                Return Me._TcpClient.Connected
            End Get
        End Property

        Sub New(tcpClient As TcpClient)
            Me._TcpClient = tcpClient
        End Sub

        Sub HandShake()
            Dim stream As NetworkStream = Me._TcpClient.GetStream()
            Dim bytes As Byte()
            Dim data As String

            While Me._TcpClient.Connected
                While (stream.DataAvailable)
                    ReDim bytes(Me._TcpClient.Client.Available)
                    stream.Read(bytes, 0, bytes.Length)
                    data = Encoding.UTF8.GetString(bytes)

                    If (New Regex("^GET").IsMatch(data)) Then

                        Dim response As Byte() = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" & Environment.NewLine & "Connection: Upgrade" & Environment.NewLine & "Upgrade: websocket" & Environment.NewLine & "Sec-WebSocket-Accept: " & Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(New Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups(1).Value.Trim() & "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) & Environment.NewLine & Environment.NewLine)

                        stream.Write(response, 0, response.Length)
                        Exit Sub
                    Else
                        'We're going to disconnect the client here, because he's not handshacking properly (or at least to the scope of this code sample)
                        Me._TcpClient.Close() 'The next While Me._TcpClient.Connected Loop Check should fail.. and raise the onClientDisconnect Event Thereafter
                    End If
                End While
            End While
            RaiseEvent onClientDisconnect()
        End Sub

        Sub doChecks()
            Dim stream As NetworkStream = Me._TcpClient.GetStream()
            Dim frameCount = 2
            Dim bytes As Byte()

            ReDim bytes(Me._TcpClient.Client.Available)
            stream.Read(bytes, 0, bytes.Length) 'Read the stream, don't close it.. 

            Dim length As UInteger = bytes(1) - 128 'this should obviously be a byte (unsigned 8bit value)

            If length > -1 Then
                If length = 126 Then
                    length = 4
                ElseIf length = 127 Then
                    length = 10
                End If
            End If

            'the following is very inefficient and likely unnecessary.. 
            'the main purpose is to just get the lower 4 bits of byte(0) - which is the OPCODE

            Dim value As Integer = bytes(0)
            Dim bitArray As BitArray = New BitArray(8)

            For c As Integer = 0 To 7 Step 1
                If value - (2 ^ (7 - c)) >= 0 Then
                    bitArray.Item(c) = True
                    value -= (2 ^ (7 - c))
                Else
                    bitArray.Item(c) = False
                End If
            Next


            Dim FRRR_OPCODE As String = ""

            For Each bit As Boolean In bitArray
                If bit Then
                    FRRR_OPCODE &= "1"
                Else
                    FRRR_OPCODE &= "0"
                End If
            Next


            Dim FIN As Integer = FRRR_OPCODE.Substring(0, 1)
            Dim RSV1 As Integer = FRRR_OPCODE.Substring(1, 1)
            Dim RSV2 As Integer = FRRR_OPCODE.Substring(2, 1)
            Dim RSV3 As Integer = FRRR_OPCODE.Substring(3, 1)
            Dim opCode As Integer = Convert.ToInt32(FRRR_OPCODE.Substring(4, 4), 2)



            Dim decoded(bytes.Length - (frameCount + 4)) As Byte

            ' 20190628 原来这里的变量名是key
            ' 并且下面的masks变量是丢失的
            Dim masks As Byte() = {bytes(frameCount), bytes(frameCount + 1), bytes(frameCount + 2), bytes(frameCount + 3)}

            Dim j As Integer = 0
            For i As Integer = (frameCount + 4) To (bytes.Length - 2) Step 1
                decoded(j) = Convert.ToByte((bytes(i) Xor masks(j Mod 4)))
                j += 1
            Next

            Call Response(opCode, decoded, stream)
        End Sub

        Private Sub Response(opCode As Integer, decoded As Byte(), stream As NetworkStream)
            Dim data As String

            Select Case opCode
                Case Is = 1
                    'Text Data Sent From Client

                    data = System.Text.Encoding.UTF8.GetString(decoded)
                    'handle this data

                    Dim Payload As Byte() = System.Text.Encoding.UTF8.GetBytes("Text Recieved: " & data)
                    Dim FRRROPCODE As Byte = Convert.ToByte("10000001", 2) 'FIN is set, and OPCODE is 1 or Text
                    Dim header As Byte() = {FRRROPCODE, Convert.ToByte(Payload.Length)}


                    Dim ResponseData As Byte()
                    ReDim ResponseData((header.Length + Payload.Length) - 1)
                    'NOTEWORTHY: if you Redim ResponseData(header.length + Payload.Length).. you'll add a 0 value byte at the end of the response data.. 
                    'which tells the client that your next stream write will be a continuation frame..

                    Dim index As Integer = 0

                    Buffer.BlockCopy(header, 0, ResponseData, index, header.Length)
                    index += header.Length

                    Buffer.BlockCopy(Payload, 0, ResponseData, index, Payload.Length)
                    index += Payload.Length
                    stream.Write(ResponseData, 0, ResponseData.Length)
                Case Is = 2
                    '// Binary Data Sent From Client 
                    data = System.Text.Encoding.UTF8.GetString(decoded)
                    Dim response As Byte() = System.Text.Encoding.UTF8.GetBytes("Binary Recieved")
                    stream.Write(response, 0, response.Length)
                Case Is = 9 '// Ping Sent From Client 
                Case Is = 10 '// Pong Sent From Client 
                Case Else '// Improper opCode.. disconnect the client 
                    _TcpClient.Close()
                    RaiseEvent onClientDisconnect()
            End Select
        End Sub

        Sub CheckForDataAvailability()
            If (Me._TcpClient.GetStream().DataAvailable) Then
                Try
                    Call doChecks()
                Catch ex As Exception
                    _TcpClient.Close()
                    RaiseEvent onClientDisconnect()
                End Try
            End If
        End Sub
    End Class

End Namespace