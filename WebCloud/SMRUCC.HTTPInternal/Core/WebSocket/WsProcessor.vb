
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Namespace Core.WebSocket
    ' https://developer.mozilla.org/zh-CN/docs/Web/API/WebSockets_API/WebSocket_Server_Vb.NET

    ''' <summary>
    ''' A websocket client
    ''' </summary>
    Public Class WsProcessor
        Dim tcpClient As TcpClient

        Public Delegate Sub OnClientDisconnectDelegateHandler()
        Public Event onClientDisconnect As OnClientDisconnectDelegateHandler

        ''' <summary>
        ''' ^GET
        ''' </summary>
        ReadOnly HttpGet As New Regex("^GET")
        ReadOnly WsSeckey As New Regex("Sec-WebSocket-Key: (.*)")
        ReadOnly sha1 As SHA1 = SHA1.Create()

        Const WsMagic As String = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"

        Public ReadOnly Property isConnected As Boolean
            Get
                Return tcpClient.Connected
            End Get
        End Property

        Sub New(tcpClient As TcpClient)
            Me.tcpClient = tcpClient
        End Sub

        Sub HandShake()
            Dim stream As NetworkStream = Me.tcpClient.GetStream()
            Dim bytes As Byte()
            Dim data As String

            While Me.tcpClient.Connected
                While (stream.DataAvailable)
                    ReDim bytes(Me.tcpClient.Client.Available)
                    stream.Read(bytes, 0, bytes.Length)
                    data = Encoding.UTF8.GetString(bytes)

                    If (HttpGet.IsMatch(data)) Then
                        Dim response As Byte()

                        SyncLock sha1
                            response = handshakePayload(data)
                            stream.Write(response, 0, response.Length)
                        End SyncLock

                        Return
                    Else
                        'We're going to disconnect the client here, because he's not handshacking properly (or at least to the scope of this code sample)
                        Me.tcpClient.Close() 'The next While Me._TcpClient.Connected Loop Check should fail.. and raise the onClientDisconnect Event Thereafter
                    End If
                End While
            End While
            RaiseEvent onClientDisconnect()
        End Sub

        Private Function handshakePayload(data As String) As Byte()
            Dim magicKey$ = WsSeckey.Match(data).Groups(1).Value.Trim() & WsMagic
            Dim verify$ = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(magicKey)))
            ' 下面的headers的文本最末尾必须以两个newline结束
            ' 所以在下面的数组之中最末尾以两个空白行结束
            Dim httpHeaders = {
                "HTTP/1.1 101 Switching Protocols",
                "Connection: Upgrade",
                "Upgrade: websocket",
                "Sec-WebSocket-Accept: " & verify,
                "",
                ""
            }

            Return Encoding.UTF8.GetBytes(httpHeaders.JoinBy(Environment.NewLine))
        End Function

        Const frameCount = 2

        Sub doChecks()
            Dim stream As NetworkStream = Me.tcpClient.GetStream()

            Dim bytes As Byte()

            ReDim bytes(Me.tcpClient.Client.Available)
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
            Dim bitArray As New BitArray(8)

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
            Dim operation As Operations = Convert.ToInt32(FRRR_OPCODE.Substring(4, 4), 2)
            Dim decoded(bytes.Length - (frameCount + 4)) As Byte

            ' 20190628 原来这里的变量名是key
            ' 并且下面的masks变量是丢失的
            Dim masks As Byte() = {
                bytes(frameCount),
                bytes(frameCount + 1),
                bytes(frameCount + 2),
                bytes(frameCount + 3)
            }

            Dim j As VBInteger = Scan0

            For i As Integer = (frameCount + 4) To (bytes.Length - 2) Step 1
                decoded(j) = Convert.ToByte((bytes(i) Xor masks(++j Mod 4)))
            Next

            Call Response(operation, decoded, stream)
        End Sub

        Private Sub Response(code As Operations, decoded As Byte(), stream As NetworkStream)
            Dim data As String

            Select Case code
                Case Is = Operations.TextRecieved
                    'Text Data Sent From Client

                    data = Encoding.UTF8.GetString(decoded)
                    'handle this data

                    Dim Payload As Byte() = Encoding.UTF8.GetBytes("Text Recieved: " & data)
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
                Case Is = Operations.BinaryRecieved
                    '// Binary Data Sent From Client 
                    data = Encoding.UTF8.GetString(decoded)
                    Dim response As Byte() = Encoding.UTF8.GetBytes("Binary Recieved")
                    stream.Write(response, 0, response.Length)
                Case Is = Operations.Ping  '// Ping Sent From Client 
                Case Is = Operations.Pong  '// Pong Sent From Client 
                Case Else '// Improper opCode.. disconnect the client 
                    tcpClient.Close()
                    RaiseEvent onClientDisconnect()
            End Select
        End Sub

        Sub CheckForDataAvailability()
            If (Me.tcpClient.GetStream().DataAvailable) Then
                Try
                    Call doChecks()
                Catch ex As Exception
                    tcpClient.Close()
                    RaiseEvent onClientDisconnect()
                End Try
            End If
        End Sub
    End Class

End Namespace