Imports System.IO
Imports System.Net.Sockets
Imports System.Text
Imports SMRUCC.REST.HttpInternal

Namespace Platform

    ''' <summary>
    ''' 服务基础类，REST API的开发需要引用当前的项目
    ''' </summary>
    Public Class PlatformEngine : Inherits HttpInternal.HttpFileSystem

        Public ReadOnly Property AppManager As AppEngine.APPManager
        Public ReadOnly Property TaskPool As New TaskPool
        Public ReadOnly Property Plugins As Plugins.PluginBase()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="port"></param>
        ''' <param name="root"></param>
        ''' <param name="nullExists"></param>
        Sub New(root As String, Optional port As Integer = 80, Optional nullExists As Boolean = False)
            Call MyBase.New(port, root, nullExists)
            Call __init()
        End Sub

        Private Sub __init()
            _AppManager = New AppEngine.APPManager(Me)
            AppEngine.ExternalCall.Scan(Me)
            Me._Plugins = REST.Platform.Plugins.ExternalCall.Scan(Me)
        End Sub

        Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As MemoryStream)
            Dim out As String = ""
            Dim args As PostParser = New PostParser(inputData, p.httpHeaders("Content-Type"), Encoding.UTF8)
            Dim success As Boolean = AppManager.InvokePOST(p.http_url, inputData, out)
            Call __handleSend(p, success, out)
        End Sub

        ''' <summary>
        ''' GET
        ''' </summary>
        ''' <param name="p"></param>
        Protected Overrides Sub __handleREST(p As HttpProcessor)
            Dim out As String = ""
            Dim success As Boolean = AppManager.Invoke(p.http_url, out)
            Call __handleSend(p, success, out)
        End Sub

        Private Sub __handleSend(p As HttpProcessor, success As Boolean, out As String)
            Call p.outputStream.WriteLine(out)

            For Each plugin As REST.Platform.Plugins.PluginBase In Plugins
                Call plugin.handleVisit(p, success)
            Next
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            For Each plugin As Plugins.PluginBase In Plugins
                Call plugin.Dispose()
            Next
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace