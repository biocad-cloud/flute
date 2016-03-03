Imports System.IO
Imports System.Net.Sockets
Imports System.Text
Imports SMRUCC.HTTPInternal.AppEngine.POSTParser
Imports SMRUCC.HTTPInternal.Core
Imports SMRUCC.HTTPInternal.Platform.Plugins

Namespace Platform

    ''' <summary>
    ''' 服务基础类，REST API的开发需要引用当前的项目
    ''' </summary>
    Public Class PlatformEngine : Inherits HttpFileSystem

        Public ReadOnly Property AppManager As AppEngine.APPManager
        Public ReadOnly Property TaskPool As New TaskPool
        Public ReadOnly Property EnginePlugins As Plugins.PluginBase()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="port"></param>
        ''' <param name="root"></param>
        ''' <param name="nullExists"></param>
        Sub New(root As String, Optional port As Integer = 80, Optional nullExists As Boolean = False, Optional appDll As String = "")
            Call MyBase.New(port, root, nullExists)
            Call __init(appDll)
        End Sub

        Private Sub __init(dll As String)
            _AppManager = New AppEngine.APPManager(Me)
            If dll.FileExists Then
                Call AppEngine.ExternalCall.ParseDll(dll, Me)
            Else
                Call AppEngine.ExternalCall.Scan(Me)
            End If
            Me._EnginePlugins = Plugins.ExternalCall.Scan(Me)
        End Sub

        Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As MemoryStream)
            Dim out As String = ""
            Dim args As PostReader = New PostReader(inputData, p.httpHeaders("Content-Type"), Encoding.UTF8)
            Dim success As Boolean = AppManager.InvokePOST(p.http_url, args, out)
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

            For Each plugin As PluginBase In EnginePlugins
                Call plugin.handleVisit(p, success)
            Next
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            For Each plugin As Plugins.PluginBase In EnginePlugins
                Call plugin.Dispose()
            Next
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace