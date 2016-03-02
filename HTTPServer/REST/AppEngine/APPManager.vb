Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.REST.HttpInternal.POSTReader
Imports SMRUCC.REST.Platform

Namespace AppEngine

    <[Namespace]("sdk")>
    Public Class APPManager : Inherits WebApp
        Implements Generic.IEnumerable(Of APPEngine)

        ''' <summary>
        ''' 键名要求是小写的
        ''' </summary>
        Dim RunningAPP As New Dictionary(Of String, APPEngine)

        Sub New(API As PlatformEngine)
            Call MyBase.New(API)
            Call Register(Me)
        End Sub

        Default Public ReadOnly Property App(name As String) As APPEngine
            Get
                name = name.ToLower
                If RunningAPP.ContainsKey(name) Then
                    Return RunningAPP(name)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public Function GetApp(Of App As Class)() As App
            Dim AppEntry = GetType(App)
            Dim LQuery = (From obj In RunningAPP.AsParallel
                          Where AppEntry.Equals(obj.Value.Application.GetType)
                          Let AppInstant = DirectCast(obj.Value.Application, App)
                          Select AppInstant).ToArray
            Return LQuery.FirstOrDefault
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of APPEngine) Implements IEnumerable(Of APPEngine).GetEnumerator
            For Each obj In RunningAPP
                Yield obj.Value
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="inputs"></param>
        ''' <param name="result">HTML输出页面或者json数据</param>
        ''' <returns></returns>
        Public Function InvokePOST(url As String, inputs As PostReader, ByRef result As String) As Boolean
            Return APPEngine.InvokePOST(url, inputs, RunningAPP, result)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="result">HTML输出页面或者json数据</param>
        ''' <returns></returns>
        Public Function Invoke(url As String, ByRef result As String) As Boolean
#Const DEBUG = 0

#If DEBUG Then
        Dim b As Boolean = APPEngine.Invoke(url, RunningAPP, result)
        result = $"<!DOCTYPE html>
            <html lang=""en"">
            	<head><title>SiYuChuangXiang (Beihai) Open Platform API Debugger</title></head>
            <body>
{result}
</body></html>"
        Return b
#Else
            Return APPEngine.Invoke(url, RunningAPP, result)
#End If
        End Function

        Public Function PrintHelp() As String
            Dim LQuery = (From app In Me.RunningAPP
                          Let head = $"<br /><div><h3>Application/Namespace                --- <strong>http://mipaimai.com/{app.Value.Namespace.Namespace}/</strong> ---</h3>" &
                          If(String.IsNullOrEmpty(app.Value.Namespace.Description), "",
                          $"                <p>{app.Value.Namespace.Description}</p>
                          <br /><br />")
                          Select head & vbCrLf &
                          app.Value.GetHelp & vbCrLf &
                          "</div>").ToArray
            Dim HelpPage As String = String.Join($"<br /><br />", LQuery)
            '    HelpPage = Program.requestHtml("sdk_doc.html").Replace("%SDK_HELP%", HelpPage)
            Return HelpPage
        End Function

        <ExportAPI("/sdk/help_doc.html", Info:="Get the help documents about how to using the mipaimai platform WebAPI.",
             Usage:="/sdk/help_doc.html",
             Example:="<a href=""/sdk/help_doc.html"">/sdk/help_doc.html</a>")>
        <APIMethods.[GET](GetType(String))>
        Public Function Help(args As String) As String
            Return PrintHelp()
        End Function

        ''' <summary>
        ''' 向开放平台之中注册API接口
        ''' </summary>
        ''' <typeparam name="APP"></typeparam>
        ''' <param name="application"></param>
        ''' <returns></returns>
        Public Function Register(Of APP As WebApp)(application As APP) As Boolean
            Dim registerApp = APPEngine.Imports(application)

            If registerApp Is Nothing Then
                Return False
            End If

            Dim hash As String = registerApp.Namespace.Namespace.ToLower
            If Me.RunningAPP.ContainsKey(hash) Then
                Return False
            Else
                Call RunningAPP.Add(hash, registerApp)
            End If

            Return True
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Overrides Function Page404() As String
            Return ""
        End Function
    End Class
End Namespace