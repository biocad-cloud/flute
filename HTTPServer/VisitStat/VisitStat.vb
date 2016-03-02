Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Parallel
Imports Oracle.LinuxCompatibility.MySQL
Imports SMRUCC.REST.HttpInternal
Imports SMRUCC.REST.Platform

Public Class VisitStat : Inherits Plugins.PluginBase

    ReadOnly _transactions As New List(Of visitor_stat)
    ReadOnly _commitThread As UpdateThread
    ReadOnly _mySQL As MySQL

    Sub New(platform As PlatformEngine)
        Call MyBase.New(platform)
        _commitThread = New UpdateThread(60 * 1000, AddressOf __commits)
    End Sub

    Private Sub __commits()
        If _transactions.IsNullOrEmpty Then
            Return
        End If

        Call _mySQL.CommitInserts(_transactions)
        Call _transactions.Clear()
    End Sub

    Public Overrides Sub handleVisit(p As HttpProcessor, success As Boolean)
        Dim ip As String = DirectCast(p.socket.Client.RemoteEndPoint, System.Net.IPEndPoint).Address.ToString
        Dim visit As New visitor_stat With {
            .ip = ip,
            .method = p.http_method,
            .success = success,
            .time = Now,
            .ua = p.httpHeaders(""),
            .url = p.http_url
        }
        Call _transactions.Add(visit)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        _commitThread.Stop()
        __commits()
        MyBase.Dispose(disposing)
    End Sub
End Class
